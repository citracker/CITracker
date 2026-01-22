using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class CIProjectToolDTO
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string Methodology { get; set; }
        public string Phase { get; set; }
        public int PhaseId { get; set; }
        public string Tool { get; set; }
        public string Url { get; set; }
        public int ToolId { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
