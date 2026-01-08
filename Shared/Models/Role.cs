using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("Role")]
    public class Role
    {
        public string Name { get; set; }
    }
}
