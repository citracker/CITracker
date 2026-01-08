using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("MethodologyPhase")]
    public class MethodologyPhase
    {
        [ExplicitKey]
        public int Id { get; set; }
        public string Methodology { get; set; }
        public string Phase { get; set; }
    }
}
