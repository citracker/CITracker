using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("Status")]
    public class Status
    {
        public string Name { get; set; }
    }
}
