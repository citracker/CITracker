using Datalayer.Interfaces;
using DocumentFormat.OpenXml.Spreadsheet;
using Infastructure.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using Shared;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infastructure.Implementation
{
    public class StripePayment : IStripePayment
    {
        private readonly ILogger<StripePayment> _logger;
        private readonly IOptions<StripeKeyValues> _config;
        private readonly IOperationManager _opsMan;

        public StripePayment(ILogger<StripePayment> logger, IOperationManager opsMan, IOptions<StripeKeyValues> config)
        {
            _logger = logger;
            _opsMan = opsMan;
            _config = config;
        }


        public async Task CancelSubscription(string subscriptionId)
        {
            try
            {
                var service = new SubscriptionService();
                await service.CancelAsync(subscriptionId, null);
            }
            catch(Exception ex) 
            { 
                _logger.LogError($"Exception at {nameof(CancelSubscription)} ||| {JsonConvert.SerializeObject(ex)}");
            }
        }

        public async Task<string> CreateCheckout(string uid, string stripeCustomerId, string stripePriceId, int qty)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    Mode = "subscription",
                    Customer = stripeCustomerId,
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Price = stripePriceId,
                            Quantity = qty
                        }
                    },
                    ClientReferenceId = uid,

                    SuccessUrl = _config.Value.SuccessCallBack,
                    CancelUrl = _config.Value.FailedCallBack
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                return session.Url;
            }
            catch (Exception ex) 
            { 
                _logger.LogError($"Exception at {nameof(CreateCheckout)} ||| {JsonConvert.SerializeObject(ex)}");
                return null;
            }
        }

        public async Task<string> CreateCustomerPortal(string customerId)
        {
            try
            {
                var options = new Stripe.BillingPortal.SessionCreateOptions
                {
                    Customer = customerId,
                    ReturnUrl = _config.Value.Dashboard
                };

                var service = new Stripe.BillingPortal.SessionService();
                var session = await service.CreateAsync(options);

                return session.Url;
            }
            catch(Exception ex) 
            { 
                _logger.LogError($"Exception at {nameof(CreateCustomerPortal)} ||| {JsonConvert.SerializeObject(ex)}");
                return null;
            }
        }

        public async Task<Customer> CreateStripeCustomer(string email, string uid)
        {
            try
            {
                var customerService = new CustomerService();

                return await customerService.CreateAsync(new CustomerCreateOptions
                {
                    Email = email,
                    Metadata = new Dictionary<string, string>
                    {
                        { "AppUserId", uid }
                    }
                });
            }
            catch (Exception ex) 
            { 
                _logger.LogError($"Exception at {nameof(CreateStripeCustomer)} ||| {JsonConvert.SerializeObject(ex)}");
                return null;
            }
        }
    }
}
