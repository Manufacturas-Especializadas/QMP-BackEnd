using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class MachineCode
    {
        public int Id { get; set; }

        public string MachineCodeName { get; set; } = string.Empty;

        public int ProcessId { get; set; }

        public Process Process { get; set; } = null!;
    }
}