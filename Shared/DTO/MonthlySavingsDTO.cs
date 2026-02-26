using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class MonthlySavingsDTO
    {
        public string month { get; set; } = default!;
        public decimal costavoidance { get; set; }
        public decimal revenue { get; set; }
        public decimal costceduction { get; set; }
        public decimal costcontainment { get; set; }
    }
}
