using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class MonthlyProjectsByMethodologyDTO
    {
        public string Month { get; set; } = default!;
        public int Dmaic { get; set; }
        public int Gemba { get; set; }
        public int Project { get; set; }
        public int Jdi { get; set; }
        public int Others { get; set; }
    }
}
