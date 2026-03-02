using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Defect
    {
        public int Id { get; set; }

        public string DefectName { get; set; } = string.Empty;

        public int TypeScrapId { get; set; }

        public TypeScrap TypeScrap { get; set; } = null!;
    }
}