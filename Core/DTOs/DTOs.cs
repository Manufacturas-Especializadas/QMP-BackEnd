using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public record LineLookupDto(int id, string Name);

    public record ShiftLookupDto(int id, string Name);

    public record MaterialLookupDto(int id, string Name);

    public record ProcessLookupDto(int id, string Name, int lineId);

    public record MachineCodeLookupDto(int id, string Name, int processId);

    public record TypeScrapLookupDto(int id, string Name);

    public record DefectsLookupDto(int id, string Name, int typeScrapId);

    public record LineCreateDto(string LineName);

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
}