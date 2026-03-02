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
}