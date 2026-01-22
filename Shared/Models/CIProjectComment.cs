using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("CIProjectComment")]
    public class CIProjectComment
    {
        [ExplicitKey]
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}