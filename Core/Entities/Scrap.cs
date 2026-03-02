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

        public string? Alloy { get; set; }

        public string? Diameter { get; set; }

        public string? Wall { get; set; }

        public string RDM { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public int ShiftId { get; set; }

        public int? ProcessId { get; set; }

        public int LineId { get; set; }

        public int MaterialId { get; set; }

        public int TypeScrapId { get; set; }

        public int DefectId { get; set; }

        public Shift Shift { get; set; } = null!;

        public Process? Process { get; set; }

        public Line Line { get; set; } = null!;

        public Material Material { get; set; } = null!;

        public TypeScrap TypeScrap { get; set; } = null!;

        public Defect Defect { get; set; } = null!;
    }
}