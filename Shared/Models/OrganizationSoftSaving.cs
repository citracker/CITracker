using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("OrganizationSoftSaving")]
    public class OrganizationSoftSaving
    {
        [ExplicitKey]
        public long Id { get; set; }
        public int OrganizationId { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
