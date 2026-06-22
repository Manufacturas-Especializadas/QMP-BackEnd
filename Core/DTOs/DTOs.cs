using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
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

    public record PipeDiametersLookupDto(int id, string Name);

    public record WallsOfDiametersLookupDto(int id, string Name);

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

    public record AvailableMonthDto(int Year, int Month, string MonthName);

    public record CreateAuditFcdsDto(
        int ShiftId,
        int FcdsProcessId,
        string PartNumber,
        List<int> LineIds,
        int? RejectionId,
        bool IsProductConforming,
        TraceabilityFcdsDto Traceability,
        ProcessControlFcdsDto Controls,
        PhysicalConditionsDto Physicals,
        List<DimensionalSpecDto>? DimensionalSpecs,
        List<VisualChecklistDto>? VisualChecklists
    );

    public record TraceabilityFcdsDto
    {
        public int AuditId { get; set; }
        public List<int> MachineCodeIds { get; init; } = new();
        public List<string> MachineCodes { get; init; } = new();
        public string OperatorsPayroll { get; init; } = string.Empty;
        public int CategoryId { get; init; }
        public int? TypeMeasuringEquipmentId { get; init; }
        public string? ShopOrder { get; init; }
        public string? BatchPipe { get; init; }
        public int? PipeDiameterId { get; init; }
        public int? PipeWallId { get; init; }
        public List<string> EquipmentSerials { get; init; } = new();
    }

    public record ProcessControlFcdsDto
    {
        public byte MttoValidation { get; init; }
        public byte Realese1stPiece { get; init; }
        public byte Spc { get; init; }
        public byte MaterialCorrectlyIdentified { get; init; }
        public byte IdentifiedMeasuringEquipment { get; init; }
        public byte CalibratedMeasuringEquipment { get; init; }
        public byte ItProcess { get; init; }
        public string TypeOil { get; init; } = string.Empty;
        public string LastHourOfRelease { get; init; } = string.Empty;
    }

    public record PhysicalConditionsDto
    {
        public byte Brands { get; init; }
        public byte Blows { get; init; }
        public byte Pollution { get; init; }
        public byte Ovality { get; init; }
        public byte Burr { get; init; }
        public byte Warped { get; init; }
        public byte ExcessOil { get; init; }
    }

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

    public record DetailedAuditFcdsDto
    {
        public int Id { get; init; }

        public DateTime? AuditDate { get; set; }
        public int ShiftId { get; init; }
        public int FcdsProcessId { get; init; }
        public string PartNumber { get; init; } = string.Empty;
        public List<string> LineNames { get; set; }  
        public List<int> LineIds { get; init; } = new();
        public bool IsProductConforming { get; init; }
        public int? RejectionId { get; init; }

        public TraceabilityFcdsDto Traceability { get; init; } = null!;
        public ProcessControlFcdsDto Controls { get; init; } = null!;
        public PhysicalConditionsDto Physicals { get; init; } = null!;
        public List<DimensionalSpecDto> DimensionalSpecs { get; init; } = new();
        public List<VisualChecklistDto> VisualChecklists { get; init; } = new();
    }

    public record AuditDataScrapReadDto(
        int Id,
        DateTime AuditDate,
        int UserId,
        string InspectorName,
        int ShiftId,
        int LeaderPayroll,
        string ShiftName,
        List<string> LineNames,
        List<int> LineIds,
        List<AuditFindingScrapReadDto> Findings
    );

    public record AuditFindingScrapReadDto(
        int Id,
        int TypeScrapId,
        string TypeScrapName,
        decimal? EstimatedWeight,
        byte MaterialCorrectlyIdentified,
        byte MaterialCorrectlySegregated,
        string? UnreportedReason,
        string? ImageEvidence,
        string? SupervisorSignature
    );


    public class CreateAuditScrapDto
    {
        public int ShiftId { get; set; }

        public int LeaderPayroll { get; set; }

        public List<int> LineIds { get; set; } = new();

        public List<CreateAuditFindingScrapDto> Findings { get; set; } = new();
    }

    public class CreateAuditFindingScrapDto
    {
        public int TypeScrapId { get; set; }

        public decimal? EstimatedWeight { get; set; }

        public byte MaterialCorrectlyIdentified { get; set; }

        public byte MaterialCorrectlySegregated { get; set; }

        public string? UnreportedReason { get; set; }

        public List<IFormFile>? ImageFiles { get; set; }

        public IFormFile? SignatureFile { get; set; }
    }

    public class UpdateAuditScrapDto
    {
        public int ShiftId { get; set; }

        public int LeaderPayroll { get; set; }

        public List<int> LineIds { get; set; } = new();

        public List<UpdateAuditFindingScrapDto> Findings { get; set; } = new();
    }

    public class UpdateAuditFindingScrapDto
    {
        public int Id { get; set; }

        public int TypeScrapId { get; set; }

        public decimal EstimatedWeight { get; set; }

        public byte MaterialCorrectlyIdentified { get; set; }

        public byte MaterialCorrectlySegregated { get; set; }

        public string? UnreportedReason { get; set; }

        public IFormFile? ImageFile { get; set; }

        public IFormFile? SignatureFile { get; set; }

        public string? KeepImageUrl { get; set; }

        public string? KeepSignatureUrl { get; set; }
    }
}