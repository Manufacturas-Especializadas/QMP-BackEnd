using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Condition
    {
        public int Id { get; set; }

        public int DefectId { get; set; }

        public string ConditionName { get; set; }

        public virtual DefectRejection DefectRejection { get; set; }
    }
}
