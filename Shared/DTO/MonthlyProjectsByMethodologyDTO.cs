using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class MonthlyProjectsByMethodologyDTO
    {
        public string month { get; set; } = default!;
        public int dmaic { get; set; }
        public int gemba { get; set; }
        public int project { get; set; }
        public int jdi { get; set; }
        public int others { get; set; }
    }
}
