using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("CIProjectTool")]
    public class CIProjectTool
    {
        [ExplicitKey]
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string Methodology { get; set; }
        public int PhaseId { get; set; }
        public int ToolId { get; set; }
        public string? Url { get; set; } //for single uploads only
        public DateTime DateCreated { get; set; }
        public long CreatedBy { get; set; }
    }
}
