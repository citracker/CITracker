using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class MethodologyMonthlyStatusDTO
    {
        public string Month { get; set; } = default!;
        public int Proposed { get; set; }
        public int Initiated { get; set; }
        public int Completed { get; set; }
        public int Closed { get; set; }
        public int Cancelled { get; set; }
    }
}
