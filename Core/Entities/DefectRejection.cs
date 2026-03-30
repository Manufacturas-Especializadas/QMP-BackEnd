using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class DefectRejection
    {
        public int Id { get; set; }

        public string DefectName { get; set; } = string.Empty;

        public virtual ICollection<Condition> Conditions { get; set; } = new List<Condition>();
    }
}