using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Line
    {
        public int Id { get; set; }

        public string LineName { get; set; } = string.Empty;

        public ICollection<Process> Processes { get; set; } = new List<Process>();

        public ICollection<Scrap> Scraps { get; set; } = new List<Scrap>();
    }
}