using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("CIProjectTeamMember")]
    public class CIProjectTeamMember
    {
        [ExplicitKey]
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public long UserId { get; set; }
        public string Role { get; set; }
        public bool SendNotification { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
