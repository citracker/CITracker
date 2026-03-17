using CITracker.Helpers;
using Datalayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared;

namespace CITracker.Controllers
{
    [Route("api/webhooks/marketplace")]
    public class MarketplaceController : Controller
    {
        private readonly ILogger<MarketplaceController> _logger;
        private readonly ISubscriptionManager _subManager;
        private readonly Mailer _mail;

        public MarketplaceController(ISubscriptionManager subManager, ILogger<MarketplaceController> logger, Mailer mail)
        {
            _subManager = subManager;
            _logger = logger;
            _mail = mail;
        }

        //[HttpPost]
        //public async Task<IActionResult> HandleWebhook([FromBody] WebhookPayload payload)
        //{
        //    // The payload contains the action (e.g., Unsubscribe, ChangePlan)
        //    switch (payload.Action)
        //    {
        //        case WebhookAction.Unsubscribe:
        //            // Deactivate the user's access in your database
        //            await DeactivateTenant(payload.SubscriptionId);
        //            break;

        //        case WebhookAction.ChangePlan:
        //            // Update the user's features/limits
        //            await UpdateTenantPlan(payload.SubscriptionId, payload.PlanId);
        //            break;
        //    }

        //    // Always return 200 OK to acknowledge receipt
        //    return Ok();
        //}
    }
}
