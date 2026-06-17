using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public record LineLookupDto(int id, string Name);

    public record ClientLookupDto(int id, string Name);

    public record ShiftLookupDto(int id, string Name);

    public record MaterialLookupDto(int id, string Name);

    public record DefectLookupDto(int id, string Name);

    public record CategoryOperatorLookupDto(int id, string Name);

    public record TypeMeasuringEquipmentLookupDto(int id, string Name);

    public record ConditionLookupDto(int id, string Name, int defectId);

    public record ProcessLookupDto(int id, string Name, int lineId);

    public record MachineCodeLookupDto(int id, string Name, int processId);

    public record MachineByLinesLookupDto(int id, string Name, int lineId);

    public record TypeScrapLookupDto(int id, string Name);

    public record ContainmentActionLookupDto(int id, string Name);

    public record DefectsLookupDto(int id, string Name, int typeScrapId);

    public record RoleReadDto(int id, string roleName);

    public record LineCreateDto(string LineName);

    public record LineReadDto(int Id, string LineName);

    public record ClientCreateDto(string ClientName);

    public record ClineReadDto(int Id, string ClientName);

    public record UserEditDto(
        int Id,
        string NewEmployeeNumber,
        int NewRoleId
    );

    public record UserRegisterDto(string EmployeeNumber, int RoleId);

    public record UserLoginDto(string EmployeeNumber, string Password);

    public record ScrapLookupDto(
        int id,
        int PayRollNumber,
        string? Alloy,
        string? Diameter,
        string? Wall,
        string RDM,
        string ShiftName,
        string ProcessName,
        string LineName,
        string MaterialName,
        string TypeScrapName,
        string? MachineCodeName,
        string DefectName,
        decimal? Weight,
        bool IsVerified,
        decimal? VerifiedWeight,
        DateTime createdAt);

    public record CreateScrapDto(
        int PayRollNumber,
        string? Alloy,
        string? Diameter,
        string? Wall,
        string RDM,
        int ShiftId,
        int? ProcessId,
        int LineId,
        int MaterialId,
        int TypeScrapId,
        int? MachineCodeId,
        int? DefectId,
        decimal Weight
    );

    public record VerifyScrapDto(int Id, bool IsVerified, decimal? VerifiedWeight);

    public record RejectionLookupDto(
        int Id,
        int? Folio,
        string Inspector,
        string? PartNumber,
        int NumberOfPieces,
        int? OperatorPayroll,
        string? Description,
        string? Image,
        string? InformedSignature,
        DateTime CreatedAt,
        string DefectName,
        string ConditionName,
        string LineName,
        string ClientName,
        string UserName,
        string ContainmentActionName,
        int? IdDefect,
        int? IdCondition,
        int? IdLine,
        int? IdClient,
        int? IdContainmentAction
    );

    public record ScrapReadDto(
        int Id,
        int PayRollNumber,
        string? Alloy,
        string? Diameter,
        string? Wall,
        string RDM,
        decimal? Weight,
        DateTime CreatedAt,
        string ShiftName,
        string LineName,
        string ProcessName,
        string? MachineCodeName,
        string TypeScrapName,
        string DefectName,
        bool IsVerified,
        decimal? VerifiedWeight
    );

    public record UsersList(int id,string payRollNumber, DateTime createdAt, bool isActive, string roleName);

    public record CreateRejectionDto(
        string Inspector,
        string PartNumber,
        int NumberOfPieces,
        int IdDefect,
        int IdCondition,
        string Description,
        int IdLine,
        int IdClient,
        int OperatorPayroll,
        int IdContainmentAction,
        int Folio,
        List<IFormFile>? Photos
    );

    public record EditRejectionDto(
        int Id,
        string Inspector,
        string PartNumber,
        int NumberOfPieces,
        int IdDefect,
        int IdCondition,
        string Description,
        int IdLine,
        int IdClient,
        int OperatorPayroll,
        int IdContainmentAction,
        int Folio,
        string? ExistingImageUrls, 
        List<IFormFile>? NewPhotos 
    );

    public record RejectionResponse(
        int Id,
        int? Folio,
        string Inspector,
        string PartNumber,
        int NumberOfPieces,
        int? OperatorPayroll,
        string Description,
        string? Image,
        string? InformedSignature,
        DateTime CreatedAt,
        string DefectName,
        string ConditionName,
        string LineName,
        string ClientName,
        string UserName,
        string ContainmentActionName,
        int? IdDefect,
        int? IdCondition,
        int? IdLine,
        int? IdClient,
        int? IdContainmentAction
    );

    public record RejectionReadDto(
        int Id,
        string Folio,
        string PartNumber,
        DateTime RegistrationDate,
        string DefectName,
        string LineName,
        string ImageUrls
    );

    public record CreateAuditFcdsDto(
        int ShiftId,
        int FcdsProcessId,
        string PartNumber,
        List<int> LineIds,
        bool IsProductConforming,
        TraceabilityFcdsDto Traceability,
        ProcessControlFcdsDto Controls,
        PhysicalConditionFcdsDto Physicals,
        List<DimensionalSpecDto>? DimensionalSpecs,
        List<VisualChecklistDto>? VisualChecklists
    );

    public record TraceabilityFcdsDto(
        int MachineCodeId,
        string OperatorsPayroll,
        int CategoryId,
        int? TypeMeasuringEquipmentId,
        string? ShopOrder,
        string? BatchPipe,
        int? PipeDiameterId,
        int? PipeWallId,
        List<string> EquipmentSerials 
    );

    public record ProcessControlFcdsDto(
        byte MttoValidation,
        byte Realese1stPiece,
        byte Spc,
        byte MaterialCorrectlyIdentified,
        byte IdentifiedMeasuringEquipment,
        byte CalibratedMeasuringEquipment,
        byte ItProcess,
        string TypeOil,
        string LastHourOfRelease
    );

    public record PhysicalConditionFcdsDto(
        byte Brands,
        byte Blows,
        byte Pollution,
        byte Ovality,
        byte Burr,
        byte Warped,
        byte ExcessOil
    );

    public record DimensionalSpecDto(
        string SpecName,                  
        string ExpectedValue,             
        string RealValue                  
    );

    public record VisualChecklistDto(
        string CheckpointName,
        byte ResultValue                  
    );

    public record AuditFcdsListDto(
        int Id,
        DateTime? AuditDate,
        string InspectorName,
        string ProcessName,
        string PartNumber,
        string LinesSummary,
        bool IsProductConforming,
        int? FolioRDM
    );
}