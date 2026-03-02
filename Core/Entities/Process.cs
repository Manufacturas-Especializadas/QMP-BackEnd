using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Process
    {
        public int Id { get; set; }

        public string ProcessName { get; set; } = string.Empty;

        public int LineId { get; set; }

        public Line Line { get; set; } = null!;

        public ICollection<MachineCode> MachineCodes { get; set; } = new List<MachineCode>();
    }
}