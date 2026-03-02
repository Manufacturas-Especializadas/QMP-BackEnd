using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public record LineLookupDto(int id, string Name);

    public record ShiftLookupDto(int id, string Name);

    public record ProcessLookupDto(int id, string Name, int lineId);
}