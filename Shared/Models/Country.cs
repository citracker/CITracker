using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("Country")]
    public class Country
    {
        [ExplicitKey] 
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
