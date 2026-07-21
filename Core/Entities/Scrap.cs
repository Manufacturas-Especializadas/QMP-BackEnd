using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Scrap
    {
        public int Id { get; set; }
        public int PayRollNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int ShiftId { get; set; }
        public int? ProcessId { get; set; }
        public int LineId { get; set; }
        public int? MachineCodeId { get; set; }

        public Shift Shift { get; set; } = null!;
        public Process? Process { get; set; }
        public Line Line { get; set; } = null!;
        public MachineCode MachineCode { get; set; } = null!;

        public ICollection<ScrapDetail> ScrapDetails { get; set; } = new List<ScrapDetail>();
    }
}