using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("OrganizationTool")]
    public class OrganizationTool
    {
        [ExplicitKey]
        public long Id { get; set; }
        public int OrganizationId { get; set; }
        public int MethodologyTool { get; set; }
        public string Url { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
