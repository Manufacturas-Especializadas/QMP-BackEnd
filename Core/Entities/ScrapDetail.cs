using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class ScrapDetail
    {
        public int Id { get; set; }

        public int ScrapId { get; set; }
        public Scrap Scrap { get; set; } = null!;

        public string? Alloy { get; set; }
        public string? Diameter { get; set; }
        public string? Wall { get; set; }
        public decimal? Weight { get; set; }
        public string RDM { get; set; } = string.Empty;
        public bool IsVerified { get; set; } = false;
        public decimal? VerifiedWeight { get; set; }

        public int MaterialId { get; set; }
        public int TypeScrapId { get; set; }
        public int? DefectId { get; set; }

        public Material Material { get; set; } = null!;
        public TypeScrap TypeScrap { get; set; } = null!;
        public Defect? Defect { get; set; }
    }
}
