using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("CIProjectComments")]
    public class CIProjectComments
    {
        [ExplicitKey]
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}