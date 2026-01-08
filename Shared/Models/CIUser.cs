using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("CIUser")]
    public class CIUser
    {
        [ExplicitKey]
        public long Id { get; set; }
        public int OrganizationId { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
