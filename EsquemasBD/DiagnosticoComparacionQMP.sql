/*
    QMP - FASE 2: diagnostico de estructuras (solo lectura)
    Compatible con SQL Server 2016 o posterior.

    INSTRUCCIONES SSMS
    1. Abrir este archivo conectado a la instancia que aloja ambas bases.
    2. Seleccionar preferentemente master en el selector de base de datos.
    3. Ajustar @SantaCatarina y, si corresponde, @EsquemaEF.
    4. Ejecutar el archivo completo (F5), con acceso y VIEW DEFINITION
       en ambas bases. No requiere modo SQLCMD.
    5. Guardar los seis conjuntos de resultados para su interpretacion.

    No contiene credenciales ni ejecuta los archivos sqlSanta/sqlRamos.
    Solo consulta catalogos sys.* y utiliza variables escalares/XML.
    No crea tablas temporales, modifica estructuras ni consulta datos
    de negocio. No ejecuta migraciones ni sincroniza datos.

    ALCANCE
    - Todas las tablas de usuario, en todos los esquemas.
    - Columnas: tipo, longitud, precision, escala, nulabilidad,
      collation, identidad y definicion calculada.
    - PK, UNIQUE, indices, FK, DEFAULT y CHECK.
    - Definicion de secuencias, sin comparar su valor actual.
    - Referencia EF: 39 tablas y cuatro columnas prioritarias.

    CRITERIOS Y LIMITES
    - Comparacion estricta de nombres y definiciones con BIN2.
      Una diferencia de mayusculas tambien se muestra.
    - Controles EF resueltos con la collation de cada base.
    - DEFAULT, CHECK y formulas calculadas se comparan literalmente:
      diferencias de formato no implican diferencias de comportamiento.
    - Los nombres automaticos de restricciones no son diferencias.
      Los indices independientes se identifican por nombre.
    - Cada diferencia cuenta una definicion/objeto, no una reparacion.
      Una tabla ausente genera tambien diferencias de sus componentes.
    - No se comparan filas, contadores actuales de identidad ni valores
      actuales de secuencias.
    - Para indices especiales se comparan metadatos comunes y se avisa
      que sus opciones especificas requieren revision adicional.
    - No audita vistas, procedimientos, triggers, permisos,
      particionamiento, compresion ni ubicacion fisica.
    - La cobertura EF de 39 tablas NO valida todas sus columnas contra
      el modelo. La validacion detallada EF aqui incluida corresponde
      a las cuatro columnas prioritarias.
    - Las capturas son consecutivas, no atomicas: ejecutar sin cambios
      de esquema concurrentes.
    - Este script no ha sido ejecutado contra las bases de produccion.
*/

SET NOCOUNT ON;

DECLARE @SantaCatarina sysname = N'QMP';       -- CONFIGURABLE
DECLARE @Ramos         sysname = N'QMP_Ramos';
DECLARE @EsquemaEF     sysname = N'dbo';       -- Segun los archivos exportados

IF DB_ID(@SantaCatarina) IS NULL OR DB_ID(@Ramos) IS NULL
BEGIN
    ;THROW 50001,
        N'Una base no existe o no es visible para esta cuenta. Revise nombres y permisos.',
        1;
END;

IF DB_ID(@SantaCatarina) = DB_ID(@Ramos)
BEGIN
    ;THROW 50002, N'Debe seleccionar dos bases distintas.', 1;
END;

IF ISNULL(HAS_DBACCESS(@SantaCatarina), 0) <> 1
   OR ISNULL(HAS_DBACCESS(@Ramos), 0) <> 1
BEGIN
    ;THROW 50003, N'No hay acceso a ambas bases.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM sys.databases
    WHERE database_id IN (DB_ID(@SantaCatarina), DB_ID(@Ramos))
      AND state_desc <> N'ONLINE'
)
BEGIN
    ;THROW 50004, N'Ambas bases deben estar ONLINE.', 1;
END;

DECLARE @MetaSanta XML;
DECLARE @MetaRamos XML;
DECLARE @Diferencias XML;
DECLARE @SQL nvarchar(max);

/*
    Se ejecuta la misma consulta en el contexto de cada base.
    QUOTENAME protege los identificadores de base.
    @EsquemaEF y la salida se pasan como parametros.
*/
DECLARE @Plantilla nvarchar(max) = N'
IF ISNULL(HAS_PERMS_BY_NAME(DB_NAME(), N''DATABASE'', N''VIEW DEFINITION''), 0) <> 1
BEGIN
    ;THROW 50005,
        N''Se requiere VIEW DEFINITION en ambas bases para evitar resultados incompletos.'',
        1;
END;

;WITH
T AS
(
    SELECT t.*, s.name AS esquema
    FROM sys.tables AS t
    INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
    WHERE t.is_ms_shipped = 0
),
C AS
(
    SELECT
        c.*,
        t.esquema,
        t.name AS tabla,
        ty.name AS tipo_nombre,
        SCHEMA_NAME(ty.schema_id) AS tipo_esquema,
        cc.definition AS calculo,
        cc.is_persisted,
        ic.seed_value,
        ic.increment_value,
        ic.is_not_for_replication AS identidad_nfr
    FROM T AS t
    INNER JOIN sys.columns AS c ON c.object_id = t.object_id
    INNER JOIN sys.types AS ty ON ty.user_type_id = c.user_type_id
    LEFT JOIN sys.computed_columns AS cc
        ON cc.object_id = c.object_id
       AND cc.column_id = c.column_id
    LEFT JOIN sys.identity_columns AS ic
        ON ic.object_id = c.object_id
       AND ic.column_id = c.column_id
),
I AS
(
    SELECT
        t.esquema,
        t.name AS tabla,
        i.*,
        claves = COALESCE
        (
            (
                SELECT
                    QUOTENAME(c.name)
                    + CASE WHEN x.is_descending_key = 1
                           THEN N'' DESC;'' ELSE N'' ASC;'' END
                FROM sys.index_columns AS x
                INNER JOIN sys.columns AS c
                    ON c.object_id = x.object_id
                   AND c.column_id = x.column_id
                WHERE x.object_id = i.object_id
                  AND x.index_id = i.index_id
                  AND x.key_ordinal > 0
                ORDER BY x.key_ordinal
                FOR XML PATH(N''''), TYPE
            ).value(N''.'', N''nvarchar(max)''),
            N''''
        ),
        incluidas = COALESCE
        (
            (
                SELECT QUOTENAME(c.name) + N'';''
                FROM sys.index_columns AS x
                INNER JOIN sys.columns AS c
                    ON c.object_id = x.object_id
                   AND c.column_id = x.column_id
                WHERE x.object_id = i.object_id
                  AND x.index_id = i.index_id
                  AND x.is_included_column = 1
                ORDER BY c.name COLLATE Latin1_General_100_BIN2
                FOR XML PATH(N''''), TYPE
            ).value(N''.'', N''nvarchar(max)''),
            N''''
        ),
        otras_columnas = COALESCE
        (
            (
                SELECT QUOTENAME(c.name) + N'';''
                FROM sys.index_columns AS x
                INNER JOIN sys.columns AS c
                    ON c.object_id = x.object_id
                   AND c.column_id = x.column_id
                WHERE x.object_id = i.object_id
                  AND x.index_id = i.index_id
                  AND x.key_ordinal = 0
                  AND x.is_included_column = 0
                ORDER BY x.index_column_id
                FOR XML PATH(N''''), TYPE
            ).value(N''.'', N''nvarchar(max)''),
            N''''
        )
    FROM T AS t
    INNER JOIN sys.indexes AS i ON i.object_id = t.object_id
    WHERE i.index_id > 0
      AND i.is_hypothetical = 0
),
F AS
(
    SELECT
        t.esquema,
        t.name AS tabla,
        fk.*,
        destino = QUOTENAME(rs.name) + N''.'' + QUOTENAME(rt.name),
        pares =
        (
            SELECT QUOTENAME(pc.name) + N''->''
                 + QUOTENAME(rc.name) + N'';''
            FROM sys.foreign_key_columns AS fc
            INNER JOIN sys.columns AS pc
                ON pc.object_id = fc.parent_object_id
               AND pc.column_id = fc.parent_column_id
            INNER JOIN sys.columns AS rc
                ON rc.object_id = fc.referenced_object_id
               AND rc.column_id = fc.referenced_column_id
            WHERE fc.constraint_object_id = fk.object_id
            ORDER BY fc.constraint_column_id
            FOR XML PATH(N''''), TYPE
        ).value(N''.'', N''nvarchar(max)'')
    FROM T AS t
    INNER JOIN sys.foreign_keys AS fk
        ON fk.parent_object_id = t.object_id
    INNER JOIN sys.tables AS rt ON rt.object_id = fk.referenced_object_id
    INNER JOIN sys.schemas AS rs ON rs.schema_id = rt.schema_id
),
Esperadas AS
(
    SELECT v.tabla
    FROM (VALUES
        (N''Users''), (N''Roles''), (N''UserRoles''),
        (N''Lines''), (N''Clients''), (N''Material''),
        (N''TypeScrap''), (N''Shifts''), (N''Process''),
        (N''MachineCodes''), (N''Defects''), (N''DefectsRejections''),
        (N''Conditions''), (N''ContainmentAction''),
        (N''CategoryOperator''), (N''TypeMeasuringEquipment''),
        (N''PipeDiameters''), (N''WallsOfDiameters''),
        (N''FCDSProcesses''), (N''Rejections''),
        (N''Scrap''), (N''ScrapDetail''),
        (N''AuditDataScrap''), (N''AuditFindingsScrap''),
        (N''AuditDataFCDS''), (N''TraceabilityElementsFCDS''),
        (N''AuditEquipmentSerialsFCDS''), (N''ProcessControlsFCDS''),
        (N''ProductReleasePhysicalCondition''),
        (N''AuditDimensionalSpecsFCDS''),
        (N''AuditVisualChecklistFCDS''),
        (N''AuditStartPoint''), (N''AuditEndPoint''),
        (N''AuditDataACD''), (N''AuditFindingsACD''),
        (N''AuditLinesFCDS''), (N''AuditMachinesFCDS''),
        (N''AudtLinesScrap''), (N''AuditLinesACD'')
    ) AS v(tabla)
),
Prioritarias AS
(
    SELECT *
    FROM (VALUES
        (N''Scrap'', N''Comments'', 231, -1, 0, 0,
         N''nvarchar(max) NULL''),
        (N''ScrapDetail'', N''Comments'', 231, -1, 0, 0,
         N''nvarchar(max) NULL''),
        (N''Rejections'', N''NumberOfInspectedPieces'', 56, 4, 10, 0,
         N''int NULL''),
        (N''AuditFindingsScrap'', N''DefectId'', 56, 4, 10, 0,
         N''int NULL'')
    ) AS v(tabla, columna, tipo_id, longitud, precision_sql, escala, esperado)
),
M AS
(
    SELECT
        CAST(N''TABLA'' AS nvarchar(40)) AS tipo,
        t.esquema,
        t.name AS tabla,
        CAST(N''-'' AS nvarchar(max)) AS elemento,
        CAST(t.name AS nvarchar(128)) AS nombre,
        CAST(CONCAT(
            N''temporal='', t.temporal_type_desc,
            N''; memoria='', t.is_memory_optimized,
            N''; durabilidad='', t.durability_desc
        ) AS nvarchar(max)) AS definicion
    FROM T AS t

    UNION ALL

    SELECT
        N''COLUMNA'', c.esquema, c.tabla, c.name, c.name,
        CONCAT(
            CAST(N''tipo='' AS nvarchar(max)),
            QUOTENAME(c.tipo_esquema), N''.'', QUOTENAME(c.tipo_nombre),
            N''; tipo_base='', TYPE_NAME(c.system_type_id),
            N''; longitud_bytes='', c.max_length,
            N''; precision='', c.precision,
            N''; escala='', c.scale,
            N''; nullable='', c.is_nullable,
            N''; collation='', COALESCE(c.collation_name, N''<no aplica>''),
            N''; identidad='', c.is_identity,
            N''; semilla='', COALESCE(CONVERT(nvarchar(100), c.seed_value), N''-''),
            N''; incremento='', COALESCE(CONVERT(nvarchar(100), c.increment_value), N''-''),
            N''; identidad_nfr='', COALESCE(CONVERT(nvarchar(1), c.identidad_nfr), N''-''),
            N''; calculada='', c.is_computed,
            N''; formula='', COALESCE(c.calculo, N''<no aplica>''),
            N''; persistida='', COALESCE(CONVERT(nvarchar(1), c.is_persisted), N''-''),
            N''; rowguid='', c.is_rowguidcol,
            N''; sparse='', c.is_sparse,
            N''; column_set='', c.is_column_set,
            N''; filestream='', c.is_filestream,
            N''; generada='', c.generated_always_type,
            N''; oculta='', c.is_hidden
        )
    FROM C AS c

    UNION ALL

    SELECT
        CASE WHEN i.is_primary_key = 1 THEN N''PK''
             WHEN i.is_unique_constraint = 1 THEN N''UNIQUE''
             ELSE N''INDICE'' END,
        i.esquema,
        i.tabla,
        CASE WHEN i.is_primary_key = 1 THEN N''PK''
             WHEN i.is_unique_constraint = 1 THEN i.claves
             ELSE i.name END,
        i.name,
        CONCAT(
            CAST(N''tipo='' AS nvarchar(max)), i.type_desc,
            N''; claves='', i.claves,
            N''; incluidas='', i.incluidas,
            N''; otras_columnas='', i.otras_columnas,
            N''; unico='', i.is_unique,
            N''; filtro='', COALESCE(i.filter_definition, N''<sin filtro>''),
            N''; deshabilitado='', i.is_disabled,
            N''; ignore_dup_key='', i.ignore_dup_key,
            N''; fill_factor='', i.fill_factor,
            N''; padded='', i.is_padded,
            N''; row_locks='', i.allow_row_locks,
            N''; page_locks='', i.allow_page_locks
        )
    FROM I AS i

    UNION ALL

    SELECT
        N''FK'', f.esquema, f.tabla,
        CONCAT(f.destino, N''|'', f.pares),
        f.name,
        CONCAT(
            CAST(N''destino='' AS nvarchar(max)), f.destino,
            N''; columnas='', f.pares,
            N''; al_borrar='', f.delete_referential_action_desc,
            N''; al_actualizar='', f.update_referential_action_desc,
            N''; deshabilitada='', f.is_disabled,
            N''; no_confiable='', f.is_not_trusted,
            N''; not_for_replication='', f.is_not_for_replication
        )
    FROM F AS f

    UNION ALL

    SELECT
        N''DEFAULT'', t.esquema, t.name, c.name, o.name,
        CONVERT(nvarchar(max), OBJECT_DEFINITION(c.default_object_id))
    FROM T AS t
    INNER JOIN sys.columns AS c ON c.object_id = t.object_id
    INNER JOIN sys.objects AS o ON o.object_id = c.default_object_id
    WHERE c.default_object_id <> 0

    UNION ALL

    SELECT
        N''CHECK'', t.esquema, t.name,
        CONVERT(nvarchar(max), ck.definition),
        ck.name,
        CONCAT(
            CAST(N''expresion='' AS nvarchar(max)), ck.definition,
            N''; columna='', COALESCE(c.name, N''<tabla>''),
            N''; deshabilitada='', ck.is_disabled,
            N''; no_confiable='', ck.is_not_trusted,
            N''; not_for_replication='', ck.is_not_for_replication,
            N''; usa_collation_bd='', ck.uses_database_collation
        )
    FROM T AS t
    INNER JOIN sys.check_constraints AS ck
        ON ck.parent_object_id = t.object_id
    LEFT JOIN sys.columns AS c
        ON c.object_id = ck.parent_object_id
       AND c.column_id = ck.parent_column_id

    UNION ALL

    SELECT
        N''SECUENCIA'', s.name, N''(secuencias)'', q.name, q.name,
        CONCAT(
            CAST(N''tipo='' AS nvarchar(max)),
            QUOTENAME(SCHEMA_NAME(ty.schema_id)), N''.'', QUOTENAME(ty.name),
            N''; precision='', q.precision,
            N''; escala='', q.scale,
            N''; inicio='', CONVERT(nvarchar(100), q.start_value),
            N''; incremento='', CONVERT(nvarchar(100), q.increment),
            N''; minimo='', CONVERT(nvarchar(100), q.minimum_value),
            N''; maximo='', CONVERT(nvarchar(100), q.maximum_value),
            N''; ciclica='', q.is_cycling,
            N''; cache='', q.is_cached,
            N''; tamano_cache='', COALESCE(CONVERT(nvarchar(100), q.cache_size), N''-'')
        )
    FROM sys.sequences AS q
    INNER JOIN sys.schemas AS s ON s.schema_id = q.schema_id
    INNER JOIN sys.types AS ty ON ty.user_type_id = q.user_type_id

    UNION ALL

    SELECT
        N''EF_TABLA'', @EsquemaEF, e.tabla, N''-'',
        COALESCE(t.name, N''<ausente>''),
        CASE WHEN t.object_id IS NULL THEN N''FALTA'' ELSE N''PRESENTE'' END
    FROM Esperadas AS e
    LEFT JOIN T AS t
        ON t.esquema = @EsquemaEF COLLATE DATABASE_DEFAULT
       AND t.name = e.tabla COLLATE DATABASE_DEFAULT

    UNION ALL

    SELECT
        N''EF_COLUMNA'', @EsquemaEF, p.tabla, p.columna,
        COALESCE(c.name, N''<ausente>''),
        CONCAT(
            CAST(N''estado='' AS nvarchar(max)),
            CASE
                WHEN t.object_id IS NULL THEN N''FALTA_TABLA''
                WHEN c.column_id IS NULL THEN N''FALTA_COLUMNA''
                WHEN c.system_type_id = p.tipo_id
                 AND c.max_length = p.longitud
                 AND c.precision = p.precision_sql
                 AND c.scale = p.escala
                 AND c.is_nullable = 1
                 AND c.is_identity = 0
                 AND c.is_computed = 0
                    THEN N''COINCIDE_TIPO_Y_NULABILIDAD''
                ELSE N''REVISAR_DEFINICION''
            END,
            N''; esperado='', p.esperado,
            N''; nombre_real='', COALESCE(c.name, N''<ausente>''),
            N''; tipo_real='', COALESCE(c.tipo_nombre, N''-''),
            N''; longitud_bytes='', COALESCE(CONVERT(nvarchar(20), c.max_length), N''-''),
            N''; precision='', COALESCE(CONVERT(nvarchar(20), c.precision), N''-''),
            N''; escala='', COALESCE(CONVERT(nvarchar(20), c.scale), N''-''),
            N''; nullable='', COALESCE(CONVERT(nvarchar(1), c.is_nullable), N''-'')
        )
    FROM Prioritarias AS p
    LEFT JOIN T AS t
        ON t.esquema = @EsquemaEF COLLATE DATABASE_DEFAULT
       AND t.name = p.tabla COLLATE DATABASE_DEFAULT
    LEFT JOIN C AS c
        ON c.object_id = t.object_id
       AND c.name = p.columna COLLATE DATABASE_DEFAULT

    UNION ALL

    SELECT
        N''AVISO_INDICE'', i.esquema, i.tabla, i.name, i.name,
        CONCAT(
            N''Tipo '', i.type_desc,
            N'': se compararon metadatos comunes; revisar opciones especificas.''
        )
    FROM I AS i
    WHERE i.type NOT IN (1, 2)
)
SELECT @Salida =
(
    SELECT tipo, esquema, tabla, elemento, nombre, definicion
    FROM M
    ORDER BY tipo, esquema, tabla, elemento, nombre
    FOR XML PATH(N''r''), ROOT(N''m''), TYPE
);
';

DECLARE @Inicio datetimeoffset = SYSDATETIMEOFFSET();

SET @SQL = N'USE ' + QUOTENAME(@SantaCatarina) + N';' + @Plantilla;

EXEC sys.sp_executesql
    @SQL,
    N'@EsquemaEF sysname, @Salida XML OUTPUT',
    @EsquemaEF = @EsquemaEF,
    @Salida = @MetaSanta OUTPUT;

SET @SQL = N'USE ' + QUOTENAME(@Ramos) + N';' + @Plantilla;

EXEC sys.sp_executesql
    @SQL,
    N'@EsquemaEF sysname, @Salida XML OUTPUT',
    @EsquemaEF = @EsquemaEF,
    @Salida = @MetaRamos OUTPUT;

DECLARE @Fin datetimeoffset = SYSDATETIMEOFFSET();

/* RESULTADO 1: servidor, bases y alcance observado. */
SELECT
    CONVERT(nvarchar(128), SERVERPROPERTY(N'ServerName')) AS ServidorInstancia,
    CONVERT(nvarchar(128), SERVERPROPERTY(N'MachineName')) AS Maquina,
    CONVERT(nvarchar(128), SERVERPROPERTY(N'InstanceName')) AS Instancia,
    CONVERT(nvarchar(128), SERVERPROPERTY(N'ProductVersion')) AS VersionSQL,
    CONVERT(nvarchar(128), SERVERPROPERTY(N'Edition')) AS Edicion,
    CASE WHEN d.database_id = DB_ID(@SantaCatarina)
         THEN N'Santa Catarina' ELSE N'Ramos' END AS Planta,
    d.name AS BaseDatos,
    d.state_desc AS Estado,
    d.collation_name AS CollationBase,
    d.compatibility_level AS Compatibilidad,
    @EsquemaEF AS EsquemaReferenciaEF,
    39 AS TablasReferenciaEF,
    CASE WHEN d.database_id = DB_ID(@SantaCatarina)
         THEN @MetaSanta.value('count(/m/r[tipo="TABLA"])', 'int')
         ELSE @MetaRamos.value('count(/m/r[tipo="TABLA"])', 'int')
    END AS TablasUsuarioObservadas,
    CASE WHEN d.database_id = DB_ID(@SantaCatarina)
         THEN @MetaSanta.value(
             'count(/m/r[tipo="EF_TABLA" and definicion="FALTA"])', 'int')
         ELSE @MetaRamos.value(
             'count(/m/r[tipo="EF_TABLA" and definicion="FALTA"])', 'int')
    END AS TablasReferenciaEFAusentes,
    @Inicio AS InicioLectura,
    @Fin AS FinLectura
FROM sys.databases AS d
WHERE d.database_id IN (DB_ID(@SantaCatarina), DB_ID(@Ramos))
ORDER BY Planta;

/*
    Emparejamiento de objetos:
      PK por tabla; UNIQUE por columnas clave;
      FK por columnas y destino; DEFAULT por columna;
      CHECK por expresion; indice independiente por nombre.
    Un CHECK modificado o una FK con columnas modificadas aparece
    como una definicion exclusiva de cada base.
    ROW_NUMBER preserva la multiplicidad de restricciones equivalentes.
*/
;WITH
Documentos AS
(
    SELECT N'S' AS origen, @MetaSanta AS documento
    UNION ALL
    SELECT N'R', @MetaRamos
),
Extraidos AS
(
    SELECT
        d.origen,
        n.r.value('(tipo/text())[1]', 'nvarchar(40)')
            COLLATE Latin1_General_100_BIN2 AS tipo,
        n.r.value('(esquema/text())[1]', 'nvarchar(128)')
            COLLATE Latin1_General_100_BIN2 AS esquema,
        n.r.value('(tabla/text())[1]', 'nvarchar(128)')
            COLLATE Latin1_General_100_BIN2 AS tabla,
        n.r.value('(elemento/text())[1]', 'nvarchar(max)')
            COLLATE Latin1_General_100_BIN2 AS elemento,
        n.r.value('(nombre/text())[1]', 'nvarchar(128)')
            COLLATE Latin1_General_100_BIN2 AS nombre,
        n.r.value('(definicion/text())[1]', 'nvarchar(max)')
            COLLATE Latin1_General_100_BIN2 AS definicion
    FROM Documentos AS d
    CROSS APPLY d.documento.nodes('/m/r') AS n(r)
),
Numerados AS
(
    SELECT *,
        ROW_NUMBER() OVER
        (
            PARTITION BY origen, tipo, esquema, tabla, elemento
            ORDER BY definicion, nombre
        ) AS repeticion
    FROM Extraidos
    WHERE tipo NOT IN (N'EF_TABLA', N'EF_COLUMNA', N'AVISO_INDICE')
),
S AS (SELECT * FROM Numerados WHERE origen = N'S'),
R AS (SELECT * FROM Numerados WHERE origen = N'R')
SELECT @Diferencias =
(
    SELECT
        COALESCE(s.tipo, r.tipo) AS tipo,
        COALESCE(s.esquema, r.esquema) AS esquema,
        COALESCE(s.tabla, r.tabla) AS tabla,
        COALESCE(s.elemento, r.elemento) AS elemento,
        COALESCE(s.repeticion, r.repeticion) AS repeticion,
        CASE
            WHEN s.origen IS NULL THEN N'SOLO_EN_RAMOS'
            WHEN r.origen IS NULL THEN N'SOLO_EN_SANTA_CATARINA'
            ELSE N'DEFINICION_DISTINTA'
        END AS estado,
        COALESCE(s.nombre, N'<ausente>') AS nombre_santa,
        COALESCE(r.nombre, N'<ausente>') AS nombre_ramos,
        COALESCE(s.definicion, N'<ausente>') AS definicion_santa,
        COALESCE(r.definicion, N'<ausente>') AS definicion_ramos
    FROM S AS s
    FULL OUTER JOIN R AS r
        ON r.tipo = s.tipo
       AND r.esquema = s.esquema
       AND r.tabla = s.tabla
       AND r.elemento = s.elemento
       AND r.repeticion = s.repeticion
    WHERE s.origen IS NULL
       OR r.origen IS NULL
       OR s.definicion <> r.definicion
    ORDER BY
        COALESCE(s.tipo, r.tipo),
        COALESCE(s.esquema, r.esquema),
        COALESCE(s.tabla, r.tabla),
        COALESCE(s.elemento, r.elemento),
        COALESCE(s.repeticion, r.repeticion)
    FOR XML PATH('r'), ROOT('diferencias'), TYPE
);

/*
    RESULTADO 2: resumen de diferencias entre bases.
    TOTAL no incluye los controles independientes contra EF.
    Si no hay diferencias, devuelve TOTAL = 0.
*/
;WITH D AS
(
    SELECT
        n.r.value('(tipo/text())[1]', 'nvarchar(40)') AS Tipo,
        n.r.value('(estado/text())[1]', 'nvarchar(40)') AS Estado
    FROM @Diferencias.nodes('/diferencias/r') AS n(r)
)
SELECT
    CASE WHEN GROUPING(Tipo) = 1 THEN N'TOTAL' ELSE Tipo END AS Tipo,
    CASE WHEN GROUPING(Estado) = 1 THEN N'TODOS' ELSE Estado END AS Estado,
    COUNT_BIG(*) AS NumeroDiferencias
FROM D
GROUP BY GROUPING SETS ((Tipo, Estado), ())
ORDER BY GROUPING(Tipo), Tipo, Estado;

/* RESULTADO 3: diferencias detalladas, por tipo y tabla. */
SELECT
    n.r.value('(tipo/text())[1]', 'nvarchar(40)') AS Tipo,
    n.r.value('(esquema/text())[1]', 'nvarchar(128)') AS Esquema,
    n.r.value('(tabla/text())[1]', 'nvarchar(128)') AS Tabla,
    n.r.value('(elemento/text())[1]', 'nvarchar(max)') AS Elemento,
    n.r.value('(repeticion/text())[1]', 'int') AS Repeticion,
    n.r.value('(estado/text())[1]', 'nvarchar(40)') AS Estado,
    @SantaCatarina AS BaseSantaCatarina,
    n.r.value('(nombre_santa/text())[1]', 'nvarchar(128)') AS NombreSanta,
    n.r.value('(definicion_santa/text())[1]', 'nvarchar(max)') AS DefinicionSanta,
    @Ramos AS BaseRamos,
    n.r.value('(nombre_ramos/text())[1]', 'nvarchar(128)') AS NombreRamos,
    n.r.value('(definicion_ramos/text())[1]', 'nvarchar(max)') AS DefinicionRamos
FROM @Diferencias.nodes('/diferencias/r') AS n(r)
ORDER BY Tipo, Esquema, Tabla, Elemento, Repeticion;

/*
    RESULTADO 4: cobertura de las 39 tablas del inventario EF.
    Devuelve 39 filas por base.
    Las tablas adicionales se incluyen en la comparacion general.
*/
;WITH Documentos AS
(
    SELECT @SantaCatarina AS BaseDatos, @MetaSanta AS documento
    UNION ALL
    SELECT @Ramos, @MetaRamos
)
SELECT
    d.BaseDatos,
    n.r.value('(esquema/text())[1]', 'nvarchar(128)') AS EsquemaEsperado,
    n.r.value('(tabla/text())[1]', 'nvarchar(128)') AS TablaEsperadaEF,
    n.r.value('(nombre/text())[1]', 'nvarchar(128)') AS NombreReal,
    n.r.value('(definicion/text())[1]', 'nvarchar(max)') AS Estado
FROM Documentos AS d
CROSS APPLY d.documento.nodes('/m/r[tipo="EF_TABLA"]') AS n(r)
ORDER BY TablaEsperadaEF, d.BaseDatos;

/*
    RESULTADO 5: columnas prioritarias. Devuelve cuatro filas por base.
    La busqueda usa la collation nativa de cada base.
    longitud_bytes: -1 = MAX; nvarchar(n) usa normalmente 2*n bytes.
*/
;WITH Documentos AS
(
    SELECT @SantaCatarina AS BaseDatos, @MetaSanta AS documento
    UNION ALL
    SELECT @Ramos, @MetaRamos
)
SELECT
    d.BaseDatos,
    n.r.value('(esquema/text())[1]', 'nvarchar(128)') AS Esquema,
    n.r.value('(tabla/text())[1]', 'nvarchar(128)') AS Tabla,
    n.r.value('(elemento/text())[1]', 'nvarchar(max)') AS ColumnaEsperadaEF,
    n.r.value('(nombre/text())[1]', 'nvarchar(128)') AS NombreReal,
    n.r.value('(definicion/text())[1]', 'nvarchar(max)') AS Diagnostico
FROM Documentos AS d
CROSS APPLY d.documento.nodes('/m/r[tipo="EF_COLUMNA"]') AS n(r)
ORDER BY Tabla, ColumnaEsperadaEF, d.BaseDatos;

/* RESULTADO 6: indices especiales que requieren revision adicional. */
;WITH Documentos AS
(
    SELECT @SantaCatarina AS BaseDatos, @MetaSanta AS documento
    UNION ALL
    SELECT @Ramos, @MetaRamos
)
SELECT
    d.BaseDatos,
    n.r.value('(esquema/text())[1]', 'nvarchar(128)') AS Esquema,
    n.r.value('(tabla/text())[1]', 'nvarchar(128)') AS Tabla,
    n.r.value('(nombre/text())[1]', 'nvarchar(128)') AS Indice,
    n.r.value('(definicion/text())[1]', 'nvarchar(max)') AS Advertencia
FROM Documentos AS d
CROSS APPLY d.documento.nodes('/m/r[tipo="AVISO_INDICE"]') AS n(r)
ORDER BY d.BaseDatos, Esquema, Tabla, Indice;

/*
    INTERPRETACION
      SOLO_EN_SANTA_CATARINA: falta esa definicion en Ramos.
      SOLO_EN_RAMOS: falta esa definicion en Santa Catarina.
      DEFINICION_DISTINTA: el elemento existe en ambas y difiere.

    Una diferencia no determina cual base es correcta.
    Cero diferencias no garantiza compatibilidad completa con EF Core.
    No se incluyen scripts de correccion.
*/
