using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infastructure.Interface
{
    public interface IStripePayment
    {
        Task<string> CreateCheckout(string userId, string priceId);
        Task CancelSubscription(string subscriptionId);
        Task<string> CreateCustomerPortal(string customerId);
    }
}
