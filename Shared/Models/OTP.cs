using Dapper.Contrib.Extensions;
using TableAttribute = Dapper.TableAttribute;


namespace Shared.Models
{
    [Table("OTP")]
    public class OTP
    {
        [ExplicitKey]
        public long Id { get; set; }
        public string UserEmail { get; set; }
        public string Value { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool Validated { get; set; }
        public DateTime? DateValidated { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedBy { get; set; }
    }
}
