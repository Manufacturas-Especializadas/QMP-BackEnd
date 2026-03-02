using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class TypeScrap
    {
        public int Id { get; set; }

        public string TypeScrapName { get; set; }

        public ICollection<Defect> Defects { get; set; } = new List<Defect>();
    }
}