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
        public int InspectorPayRollNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int ShiftId { get; set; }
        public int LineId { get; set; }
        public Shift Shift { get; set; } = null!;
        public Line Line { get; set; } = null!;
        public bool IsVerified { get; set; } = false;
        public decimal? VerifiedWeight { get; set; }
        public decimal TotalWeight { get; set; }

        public ICollection<ScrapDetail> ScrapDetails { get; set; } = new List<ScrapDetail>();
    }
}