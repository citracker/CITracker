using CITracker.Helpers;
using Datalayer.Interfaces;
using Infastructure.Interface;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared;
using Shared.Enumerations;
using Shared.ExternalModels;

namespace CITracker.Controllers
{
    [Route("api/webhooks/marketplace")]
    public class MarketplaceController : Controller
    {
        private readonly ILogger<MarketplaceController> _logger;
        private readonly ISubscriptionManager _subManager;
        private readonly IMicrosoftOperations _msOps;
        private readonly IOptions<ADKeyValues> _config;
        private readonly Mailer _mail;

        public MarketplaceController(ISubscriptionManager subManager, IMicrosoftOperations msOps, ILogger<MarketplaceController> logger, IOptions<ADKeyValues> config, Mailer mail)
        {
            _subManager = subManager;
            _logger = logger;
            _mail = mail;
            _msOps = msOps;
            _config = config;
        }

        public async Task HandleWebhook(Webhook payload)
        {
            _logger.LogInformation($"Received webhook with action: {payload.Action} for subscription: {payload.SubscriptionId} ||| {JsonConvert.SerializeObject(payload)}");

            var subscription = await _msOps.GetSubscription(payload.SubscriptionId, _config.Value.TenantId);

            switch (payload.Action)
            {
                case "Unsubscribe":
                case "Suspend":
                    DeactivateOrDisable(subscription);
                    break;

                case "Reinstate":
                    Enable(subscription);
                    break;

                case "ChangePlan":
                    UpdatePlan(subscription);
                    break;
            }
        }


        private async Task DeactivateOrDisable(CIMarketplaceSubscription subscription)
        {
            _logger.LogInformation($"Unsubscribe or Suspend Event hit |||  {JsonConvert.SerializeObject(subscription)}");

            //await _subManager.MPUpdateOrganizationSubscription(); //Deactivate or Disable
        }


        private async Task Enable(CIMarketplaceSubscription subscription)
        {
            _logger.LogInformation($"Reinstate Event hit |||  {JsonConvert.SerializeObject(subscription)}");

            //await _subManager.MPUpdateOrganizationSubscription(); //Enable user Account
        }


        private async Task UpdatePlan(CIMarketplaceSubscription subscription)
        {
            _logger.LogInformation($"UpdatePlan Event hit |||  {JsonConvert.SerializeObject(subscription)}");

            //await _subManager.MPUpdateOrganizationSubscription(); //Update user Subscription Plan
        }
    }
}
