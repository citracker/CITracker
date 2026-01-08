
using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("OperationalExcellenceMonthlySaving")]
    public class OperationalExcellenceMonthlySaving
    {
        [ExplicitKey]
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public int OrganizationId { get; set; }
        public string MonthYear { get; set; }
        public decimal Savings { get; set; }
        public string Currency { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
