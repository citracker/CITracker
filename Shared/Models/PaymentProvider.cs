using Dapper.Contrib.Extensions;

namespace Shared.Models
{
    [Table("PaymentProvider")]
    public class PaymentProvider
    {
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public string LogoType { get; set; }
        public bool IsActive { get; set; }
    }
}
