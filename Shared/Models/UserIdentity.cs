using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("UserIdentity")]
    public class UserIdentity
    {
        [ExplicitKey]
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Provider { get; set; }
        public string ExternalId { get; set; }
        public string Email { get; set; }
        public string TenantHint { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
