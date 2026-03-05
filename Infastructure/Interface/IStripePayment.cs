using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infastructure.Interface
{
    public interface IStripePayment
    {
        Task<string> CreateCheckout(string uid, string stripeCustomerId, string stripePriceId, int qty);
        Task CancelSubscription(string subscriptionId);
        Task<string> CreateCustomerPortal(string customerId);
        Task<Customer> CreateStripeCustomer(string email, string uid);
    }
}
