using ClosedXML.Excel;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ExcelService : IExcelService
    {
        public byte[] GenerateScrapReport(IEnumerable<ScrapFlatExportDto> data)
        {
            using (var workbook = new XLWorkbook())
            {

                var wsHeaders = workbook.Worksheets.Add("Reportes Generales");

                var cabeceraHeaders = new string[] {
            "ID Reporte", "PE Inspector", "Fecha", "Línea", "Turno",
            "Verificado", "Peso Total", "Peso Verificado"
        };

                for (int i = 0; i < cabeceraHeaders.Length; i++)
                {
                    var cell = wsHeaders.Cell(1, i + 1);
                    cell.Value = cabeceraHeaders[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
                    cell.Style.Font.FontColor = XLColor.White;
                }


                var uniqueScraps = data.GroupBy(x => x.ScrapId).Select(g => g.First());

                int rowCabecera = 2;
                foreach (var item in uniqueScraps)
                {
                    wsHeaders.Cell(rowCabecera, 1).Value = item.ScrapId;
                    wsHeaders.Cell(rowCabecera, 2).Value = item.InspectorPayRollNumber;
                    wsHeaders.Cell(rowCabecera, 3).Value = item.CreatedAt.ToString("g");
                    wsHeaders.Cell(rowCabecera, 4).Value = item.LineName;
                    wsHeaders.Cell(rowCabecera, 5).Value = item.ShiftName;
                    wsHeaders.Cell(rowCabecera, 6).Value = item.IsVerified ? "Sí" : "No";
                    wsHeaders.Cell(rowCabecera, 7).Value = item.TotalWeight;
                    wsHeaders.Cell(rowCabecera, 8).Value = item.VerifiedWeight;
                    rowCabecera++;
                }


                var wsDetails = workbook.Worksheets.Add("Detalles de Scrap");

                var detailHeaders = new string[] {
             "ID Detalle", "PE Operador", "Proceso", "Código Máquina",
            "Material", "Aleación", "Diámetro", "Pared", "Defecto", "Peso", "RDM", "Num Parte"
                };

                for (int i = 0; i < detailHeaders.Length; i++)
                {
                    var cell = wsDetails.Cell(1, i + 1);
                    cell.Value = detailHeaders[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#047857"); 
                    cell.Style.Font.FontColor = XLColor.White;
                }

                int rowDetail = 2;
                foreach (var item in data)
                {
                    
                    wsDetails.Cell(rowDetail, 1).Value = item.ScrapId; 
                    wsDetails.Cell(rowDetail, 2).Value = item.PayRollNumber;
                    wsDetails.Cell(rowDetail, 3).Value = item.ProcessName;
                    wsDetails.Cell(rowDetail, 4).Value = item.MachineCodeName;
                    wsDetails.Cell(rowDetail, 5).Value = item.Material;
                    wsDetails.Cell(rowDetail, 6).Value = item.Alloy;
                    wsDetails.Cell(rowDetail, 7).Value = item.Diameter;
                    wsDetails.Cell(rowDetail, 8).Value = item.Wall;
                    wsDetails.Cell(rowDetail, 9).Value = item.DefectName;
                    wsDetails.Cell(rowDetail, 10).Value = item.Weight;
                    wsDetails.Cell(rowDetail, 11).Value = item.RDM;
                    wsDetails.Cell(rowDetail, 12).Value = item.PartNumber;
                    rowDetail++;
                }

                wsHeaders.Columns().AdjustToContents();
                wsDetails.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public byte[] GenerateRejectionReport(IEnumerable<RejectionResponse> data)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Reporte de rechazos");
                var headers = new string[] 
                {
                    "ID", "Folio", "Fecha", "Inspector", "Nro Parte",
                    "Piezas", "Línea", "Cliente", "Defecto", "Condición", "Acción",
                    "Nómina del operador", "Descripción detallada del rechazo",
                    "Evidencia", "Firma"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                int row = 2;
                using var httpClient = new HttpClient();

                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.Id;
                    worksheet.Cell(row, 2).Value = item.Folio;
                    worksheet.Cell(row, 3).Value = item.CreatedAt.ToString("g");
                    worksheet.Cell(row, 4).Value = item.Inspector;
                    worksheet.Cell(row, 5).Value = item.PartNumber;
                    worksheet.Cell(row, 6).Value = item.NumberOfPieces;
                    worksheet.Cell(row, 7).Value = item.LineName;
                    worksheet.Cell(row, 8).Value = item.ClientName;
                    worksheet.Cell(row, 9).Value = item.DefectName;
                    worksheet.Cell(row, 10).Value = item.ConditionName;
                    worksheet.Cell(row, 11).Value = item.ContainmentActionName;
                    worksheet.Cell(row, 12).Value = item.OperatorPayroll;
                    worksheet.Cell(row, 13).Value = item.Description;
                    worksheet.Row(row).Height = 70;

                    if (!string.IsNullOrEmpty(item.Image))
                    {
                        try
                        {
                            var allUrls = item.Image.Split(';', StringSplitOptions.RemoveEmptyEntries).Take(3);

                            int columnOffset = 0;
                            foreach (var url in allUrls)
                            {
                                byte[] imgBytes = httpClient.GetByteArrayAsync(url).Result;
                                using var ms = new MemoryStream(imgBytes);

                                var pic = worksheet.AddPicture(ms)
                                    .MoveTo(worksheet.Cell(row, 14))
                                    .WithSize(80, 80);

                                pic.Left = 5 + (columnOffset * 85);
                                pic.Top = 5;

                                columnOffset++;
                            }
                        }
                        catch {  }
                    }

                    if (!string.IsNullOrEmpty(item.InformedSignature))
                    {
                        try
                        {
                            byte[] sigBytes = httpClient.GetByteArrayAsync(item.InformedSignature).Result;
                            using var msSig = new MemoryStream(sigBytes);
                            var sig = worksheet.AddPicture(msSig)
                                .MoveTo(worksheet.Cell(row, 15))
                                .WithSize(90, 45);
                        }
                        catch { }
                    }

                    row++;
                }

                worksheet.Columns().AdjustToContents();
                worksheet.Column(12).Width = 40;
                worksheet.Column(13).Width = 15;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public byte[] GenerateAuditsFcdsReport(IEnumerable<DetailedAuditFcdsDto> data)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Auditorías FCD");

                var headers = new string[]
                {
                    "ID", "Fecha Registro", "Turno", "Proceso", "No. Parte", "Líneas Auditadas", "Estatus", "Folio RDM",
                    "Nómina Operador", "Cat. Operador", "Shop Order", "Lote Tubería", "Máquinas", "Series Equipos",
                    "Validación Mtto", "1ra Pieza", "SPC", "Material Identificado", "Equipo Identificado", "Equipo Calibrado", "IT Proceso", "Tipo Aceite", "Hora Liberación",
                    "Marcas", "Golpes", "Contaminación", "Ovalamiento", "Rebaba", "Pandeado", "Exceso Aceite",
                    "Mediciones Dimensionales / Checklist Visual", "Nomina Inspector", "Adecuado Para La Medición", "Corresponde Al Operador Auditado" 
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                string TraducirControl(int val) => val == 1 ? "CUMPLE" : val == 2 ? "NO CUMPLE" : "N/A";
                string TraducirFisico(int val) => val == 1 ? "DETECTADO" : val == 2 ? "LIMPIO" : "N/A";

                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.Id;
                    worksheet.Cell(row, 2).Value = item.AuditDate;
                    worksheet.Cell(row, 3).Value = item.ShiftId == 1 ? "Día" : item.ShiftId == 2 ? "Tarde" : "Noche";
                    worksheet.Cell(row, 4).Value = item.ProcessName ?? "-";
                    worksheet.Cell(row, 5).Value = item.PartNumber;
                    worksheet.Cell(row, 6).Value = string.Join(", ", item.LineNames ?? new List<string>());
                    worksheet.Cell(row, 7).Value = item.IsProductConforming ? "CONFORME" : "NO CONFORME";
                    worksheet.Cell(row, 8).Value = item.RejectionId.HasValue ? item.RejectionId.Value.ToString() : "—";

                    worksheet.Cell(row, 9).Value = item.Traceability?.OperatorsPayroll ?? "—";
                    worksheet.Cell(row, 10).Value = item.Traceability?.CategoryId ?? 0;
                    worksheet.Cell(row, 11).Value = item.Traceability?.ShopOrder ?? "—";
                    worksheet.Cell(row, 12).Value = item.Traceability?.BatchPipe ?? "—";
                    worksheet.Cell(row, 13).Value = string.Join(", ", item.Traceability?.MachineCodes ?? new List<string>());
                    worksheet.Cell(row, 14).Value = string.Join(", ", item.Traceability?.EquipmentSerials ?? new List<string>());

                    worksheet.Cell(row, 15).Value = TraducirControl(item.Controls?.MttoValidation ?? 0);
                    worksheet.Cell(row, 16).Value = TraducirControl(item.Controls?.Realese1stPiece ?? 0);
                    worksheet.Cell(row, 17).Value = TraducirControl(item.Controls?.Spc ?? 0);
                    worksheet.Cell(row, 18).Value = TraducirControl(item.Controls?.MaterialCorrectlyIdentified ?? 0);
                    worksheet.Cell(row, 19).Value = TraducirControl(item.Controls?.IdentifiedMeasuringEquipment ?? 0);
                    worksheet.Cell(row, 20).Value = TraducirControl(item.Controls?.CalibratedMeasuringEquipment ?? 0);
                    worksheet.Cell(row, 21).Value = TraducirControl(item.Controls?.ItProcess ?? 0);
                    worksheet.Cell(row, 22).Value = item.Controls?.TypeOil ?? "—";
                    worksheet.Cell(row, 23).Value = item.Controls?.LastHourOfRelease ?? "—";

                    worksheet.Cell(row, 24).Value = TraducirFisico(item.Physicals?.Brands ?? 0);
                    worksheet.Cell(row, 25).Value = TraducirFisico(item.Physicals?.Blows ?? 0);
                    worksheet.Cell(row, 26).Value = TraducirFisico(item.Physicals?.Pollution ?? 0);
                    worksheet.Cell(row, 27).Value = TraducirFisico(item.Physicals?.Ovality ?? 0);
                    worksheet.Cell(row, 28).Value = TraducirFisico(item.Physicals?.Burr ?? 0);
                    worksheet.Cell(row, 29).Value = TraducirFisico(item.Physicals?.Warped ?? 0);
                    worksheet.Cell(row, 30).Value = TraducirFisico(item.Physicals?.ExcessOil ?? 0);
                    


                    if (item.DimensionalSpecs != null && item.DimensionalSpecs.Any())
                    {
                        var specsSummary = item.DimensionalSpecs.Select(s => $"{s.SpecName}[Esp:{s.ExpectedValue} Real:{s.RealValue}]");
                        worksheet.Cell(row, 31).Value = string.Join(" | ", specsSummary);
                    }
                    else if (item.VisualChecklists != null && item.VisualChecklists.Any())
                    {
                        var visualSummary = item.VisualChecklists.Select(v => $"{v.CheckpointName}: {TraducirControl(v.ResultValue)}");
                        worksheet.Cell(row, 31).Value = string.Join(" | ", visualSummary);
                    }
                    else
                    {
                        worksheet.Cell(row, 31).Value = "—";
                    }
                    worksheet.Cell(row, 32).Value = item.InspectorPayroll ?? "-";
                    worksheet.Cell(row, 33).Value = TraducirControl(item.Controls?.MeasuringEquipmentAdequate ?? 0);
                    worksheet.Cell(row, 34).Value = TraducirControl(item.Controls?.MeasuringEquipmentOperatorMatch ?? 0);

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public byte[] GenerateAuditScrapReport(IEnumerable<AuditDataScrap> data)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Auditorías de Scrap");

                var headers = new string[]
                {
                    "ID Audit", "Fecha Registro", "Inspector", "Turno", "Nómina Líder",
                    "Líneas Auditadas", "Tipo de Scrap", "Peso Estimado (KG)",
                    "Identificación Material", "Segregación Contenedor", "Motivo de Anomalía"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                string TraducirControl(int val) => val == 1 ? "CUMPLE" : val == 2 ? "NO CUMPLE" : "N/A";

                int row = 2;
                foreach (var audit in data)
                {
                    string lineasConcat = string.Join(", ", audit.Lines.Select(l => l.LineName));

                    if (audit.Findings == null || !audit.Findings.Any())
                    {
                        worksheet.Cell(row, 1).Value = audit.Id;
                        worksheet.Cell(row, 2).Value = audit.AuditDate.ToString("g");
                        worksheet.Cell(row, 3).Value = audit.User?.Username ?? "N/A";
                        worksheet.Cell(row, 4).Value = audit.Shift?.ShiftName ?? "N/A";
                        worksheet.Cell(row, 5).Value = audit.LeaderPayroll;
                        worksheet.Cell(row, 6).Value = lineasConcat;
                        worksheet.Cell(row, 7).Value = "—";
                        worksheet.Cell(row, 8).Value = 0;
                        worksheet.Cell(row, 9).Value = "—";
                        worksheet.Cell(row, 10).Value = "—";
                        worksheet.Cell(row, 11).Value = "—";
                        row++;
                        continue;
                    }

                    foreach (var finding in audit.Findings)
                    {
                        worksheet.Cell(row, 1).Value = audit.Id;
                        worksheet.Cell(row, 2).Value = audit.AuditDate;
                        worksheet.Cell(row, 3).Value = audit.User?.Username ?? "N/A";
                        worksheet.Cell(row, 4).Value = audit.Shift?.ShiftName ?? "N/A";
                        worksheet.Cell(row, 5).Value = audit.LeaderPayroll;
                        worksheet.Cell(row, 6).Value = lineasConcat;

                        worksheet.Cell(row, 7).Value = finding.TypeScrap?.TypeScrapName ?? "N/A";
                        worksheet.Cell(row, 8).Value = finding.EstimatedWeight;
                        worksheet.Cell(row, 9).Value = TraducirControl(finding.MaterialCorrectlyIdentified);
                        worksheet.Cell(row, 10).Value = TraducirControl(finding.MaterialCorrectlySegregated);
                        worksheet.Cell(row, 11).Value = !string.IsNullOrEmpty(finding.UnreportedReason) ? finding.UnreportedReason : "—";

                        row++;
                    }
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        private string ObtenerTextoDeEvaluacion(byte valor)
        {
            return valor switch
            {
                1 => "Cumple",
                2 => "No Cumple",   
                3 => "No Aplica",    
                _ => "Sin evaluar"   
            };
        }

        public byte[] GenerateACDReport(IEnumerable<AuditDataACD> data)
        {
            using (var workbook = new XLWorkbook())
            {
                var wsHeaders = workbook.Worksheets.Add("ACD Report");

                var cabeceraHeaders = new string[] {
                    "ID", "Fecha", "Turno", "Línea", "Inspector"," No. De Parte",
                    "Nomina Del Empacador", "Cant. De Piezas", "Tamaño De Muestra",
                    "Shop Order", "Idetificacion Correcta (ID contenedor Vs ID pieza)", "Procesos Completos",
                    "PP Segun BOM", "Defectos De Soldadura", "Vista frontal", "Vista Lateral",
                    "Vista Superior", "Vista Isometrica", "Estatus"
                };

                for (int i = 0; i < cabeceraHeaders.Length; i++)
                {
                    var cell = wsHeaders.Cell(1, i + 1);
                    cell.Value = cabeceraHeaders[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
                    cell.Style.Font.FontColor = XLColor.White;
                }

                var uniqueScraps = data.GroupBy(x => x.Id).Select(g => g.First());

                int rowCabecera = 2;
                foreach (var item in uniqueScraps)
                {
                    string lineasStr = item.Lines != null && item.Lines.Any()
                        ? string.Join(", ", item.Lines.Select(l => l.LineName))
                        : "Sin Línea";

                    
                    string inspector = item.User != null ? item.User.Username.ToString() : "N/D";

                    
                    if (item.Findings != null && item.Findings.Any())
                    {
                        
                        foreach (var finding in item.Findings)
                        {
                            
                            wsHeaders.Cell(rowCabecera, 1).Value = item.Id;
                            wsHeaders.Cell(rowCabecera, 2).Value = item.AuditDate.ToString("yyyy-MM-dd HH:mm");
                            wsHeaders.Cell(rowCabecera, 3).Value = item.Shift?.ShiftName ?? "N/D";
                            wsHeaders.Cell(rowCabecera, 4).Value = lineasStr;
                            wsHeaders.Cell(rowCabecera, 5).Value = inspector;

                            
                            wsHeaders.Cell(rowCabecera, 6).Value = finding.PartNumber;
                            wsHeaders.Cell(rowCabecera, 7).Value = finding.PackerPayroll;
                            wsHeaders.Cell(rowCabecera, 8).Value = finding.NumberOfPieces;
                            wsHeaders.Cell(rowCabecera, 9).Value = finding.SampleSize;
                            wsHeaders.Cell(rowCabecera, 10).Value = finding.ShopOrder ?? "N/D";

                            
                            wsHeaders.Cell(rowCabecera, 11).Value = finding.ContainerIdMatch == true ? "Sí" : "No";
                            wsHeaders.Cell(rowCabecera, 12).Value = finding.CompleteProcess == true ? "Sí" : "No";

                            wsHeaders.Cell(rowCabecera, 13).Value = finding.PpBom == 1 ? "Sí" : "No";
                            wsHeaders.Cell(rowCabecera, 14).Value = finding.WeldingDefects == 1 ? "Sí" : "No";
                            wsHeaders.Cell(rowCabecera, 15).Value = ObtenerTextoDeEvaluacion(finding.FrontView);
                            wsHeaders.Cell(rowCabecera, 16).Value = ObtenerTextoDeEvaluacion(finding.SideView);
                            wsHeaders.Cell(rowCabecera, 17).Value = ObtenerTextoDeEvaluacion(finding.TopView);
                            wsHeaders.Cell(rowCabecera, 18).Value = ObtenerTextoDeEvaluacion(finding.IsometricView);


                            wsHeaders.Cell(rowCabecera, 19).Value = finding.IsProductConforming ? "Conforme" : "No Conforme";

                            rowCabecera++;
                        }
                    }
                    else
                    {
                        
                        wsHeaders.Cell(rowCabecera, 1).Value = item.Id;
                        wsHeaders.Cell(rowCabecera, 2).Value = item.AuditDate.ToString("yyyy-MM-dd HH:mm");
                        wsHeaders.Cell(rowCabecera, 3).Value = item.Shift?.ShiftName ?? "N/D";
                        wsHeaders.Cell(rowCabecera, 4).Value = lineasStr;
                        wsHeaders.Cell(rowCabecera, 5).Value = inspector;

                        rowCabecera++;
                    }
                }

                
                wsHeaders.Columns().AdjustToContents();


                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }



    }
}