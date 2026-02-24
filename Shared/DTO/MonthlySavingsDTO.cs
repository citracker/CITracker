using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class MonthlySavingsDTO
    {
        public string Month { get; set; } = default!;
        public decimal CostAvoidance { get; set; }
        public decimal Revenue { get; set; }
        public decimal CostReduction { get; set; }
        public decimal CostContainment { get; set; }
    }
}
