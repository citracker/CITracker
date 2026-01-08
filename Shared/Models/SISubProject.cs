
using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("SISubProject")]
    public class SISubProject
    {
        [ExplicitKey]
        public long Id { get; set; }
        public long SIId { get; set; }
        public string Initiative { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public long FacilitatorId { get; set; }
        public decimal Percentage { get; set; }
        public decimal Savings { get; set; }
        public string Currency { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
