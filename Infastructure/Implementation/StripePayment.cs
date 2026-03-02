using Datalayer.Interfaces;
using Infastructure.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared;
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
            throw new NotImplementedException();
        }

        public async Task<string> CreateCheckout(string userId, string priceId)
        {
            throw new NotImplementedException();
        }

        public async Task<string> CreateCustomerPortal(string customerId)
        {
            throw new NotImplementedException();
        }
    }
}
