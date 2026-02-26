using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class MethodologyMonthlyStatusDTO
    {
        public string month { get; set; } = default!;
        public int proposed { get; set; }
        public int initiated { get; set; }
        public int completed { get; set; }
        //public int closed { get; set; }
        //public int cancelled { get; set; }
    }
}
