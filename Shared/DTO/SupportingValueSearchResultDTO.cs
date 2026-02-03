using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class SupportingValueSearchResultDTO
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Source { get; set; } // "OE" or "SI"
    }
}
