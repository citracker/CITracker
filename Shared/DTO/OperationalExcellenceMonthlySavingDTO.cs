using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class OperationalExcellenceMonthlySavingDTO
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public int OrganizationId { get; set; }
        public string MonthYear { get; set; }
        public decimal Savings { get; set; }
        public string Currency { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
        public string CreatedByUser { get; set; }
    }
}
