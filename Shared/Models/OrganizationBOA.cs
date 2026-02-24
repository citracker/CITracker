using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("OrganizationBOA")]
    public class OrganizationBOA
    {
        [ExplicitKey]
        public long Id { get; set; }
        public int OrganizationId { get; set; }
        public string BOA { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
