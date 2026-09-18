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
using System.Text;

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

        [HttpPost]
        public async Task<IActionResult> HandleWebhook([FromBody] Webhook payload)
        {
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            var raw = await reader.ReadToEndAsync();
            Request.Body.Position = 0;
            _logger.LogInformation($"RAW WEBHOOK BODY: {raw}");


            if (payload == null)
            {
                _logger.LogWarning("Webhook received but payload was null.");
                return BadRequest();
            }

            _logger.LogInformation($"Received webhook. Action={payload.MarketplaceAction}, SubId={payload.SubscriptionId} ||| {JsonConvert.SerializeObject(payload)}");

            var subscription = await _msOps.GetSubscription(payload.SubscriptionId, _config.Value.CITenantId);

            switch (payload.MarketplaceAction)
            {
                case "Unsubscribe":
                case "Suspended":
                    await DeactivateOrDisable(subscription);
                    break;

                case "Reinstate":
                    await Enable(subscription);
                    break;

                case "ChangePlan":
                    await UpdatePlan(subscription);
                    break;
            }

            return Ok();
        }


        private async Task DeactivateOrDisable(CIMarketplaceSubscription subscription)
        {
            _logger.LogInformation($"Unsubscribe or Suspend Event hit |||  {JsonConvert.SerializeObject(subscription)}");

            await _subManager.MPDeactivateOrganizationSubscription(subscription); //Deactivate or Disable
        }


        private async Task Enable(CIMarketplaceSubscription subscription)
        {
            _logger.LogInformation($"Reinstate Event hit |||  {JsonConvert.SerializeObject(subscription)}");

            await _subManager.UpdateOrganizationSubscriptionFromMPEvent(subscription); //Enable user Account
        }


        private async Task UpdatePlan(CIMarketplaceSubscription subscription)
        {
            _logger.LogInformation($"UpdatePlan Event hit |||  {JsonConvert.SerializeObject(subscription)}");

            await _subManager.UpdateOrganizationSubscriptionFromMPEvent(subscription); //Update user Subscription Plan
        }
    }
}
