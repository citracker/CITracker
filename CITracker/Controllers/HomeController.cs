using CITracker.Helpers;
using Datalayer.Interfaces;
using Infastructure.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using Shared;
using Shared.DTO;
using Shared.Enumerations;
using Shared.Models;
using Shared.Utilities;
using Shared.ViewModels;
using System.Net;
using Organization = Shared.Models.Organization;
using Subscription = Shared.Models.Subscription;

namespace CITracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISubscriptionManager _subManager;
        private readonly IPaymentManager _payManager;
        private readonly IOperationManager _opsManager;
        private readonly IUserManager _usrManager;
        private readonly IMicrosoftOperations _msOps;
        private readonly IStripePayment _strPay;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IOptions<KeyValues> _config;
        private readonly IOptions<ADKeyValues> _adconfig;
        private readonly Mailer _mail;

        public HomeController(ILogger<HomeController> logger, IOptions<KeyValues> config, IOptions<ADKeyValues> adconfig, ISubscriptionManager subManager, IPaymentManager payManager, IOperationManager opsManager, IUserManager usrManager, Mailer mail, IMicrosoftOperations msOps, ITokenAcquisition tokenAcquisition, IStripePayment strPay)
        {
            _logger = logger;
            _subManager = subManager;
            _payManager = payManager;
            _opsManager = opsManager;
            _usrManager = usrManager;
            _mail = mail;
            _config = config;
            _msOps = msOps;
            _tokenAcquisition = tokenAcquisition;
            _strPay = strPay;
            _adconfig = adconfig;
        }


        [HttpGet("")]
        public IActionResult Index()
        {
            // 1. Not authenticated → show public landing / pricing
            if (!IsAuthenticated())
                return View(_subManager.GetAllSubscriptionPlans().Result);

            // 2. Resolve identity from whichever provider signed this user in
            var identity = ResolveIdentityContext();


            _logger.LogInformation($"Authenticated user {identity.Email} via {identity.Provider}");

            // 3. Seed session identity keys FIRST so SetSessionVariables doesn't clobber them
            HttpContext.Session.SetString("UserEmail", identity.Email);
            HttpContext.Session.SetString("UserName", $"{identity.FirstName} {identity.LastName}");
            HttpContext.Session.SetString("IdentityProvider", identity.Provider);
            HttpContext.Session.SetString("ExternalId", identity.ExternalId);
            HttpContext.Session.SetString("TenantId", identity.OrganizationKey ?? "");   // empty for Google

            // 4. Look up the CIUser row for this email (works for all providers)
            var userResp = _usrManager.GetUserByEmail(identity.Email).Result;

            // 5. Case A: user exists in CIUser → they belong to an Organization
            if (userResp != null && userResp.StatusCode == (int)HttpStatusCode.OK && userResp.SingleResult != null)
            {
                var org = userResp.SingleResult;

                // For Google users, derive the tenant from the org (they have no tid claim)
                var tenantForLookup = !string.IsNullOrEmpty(identity.OrganizationKey) ? identity.OrganizationKey : org.OrganizationDomain;   // or expose Organization.TenantId on the DTO

                // 5a. Fetch subscription status for that tenant
                var orgDetails = _subManager.GetOrganizationSubscription(tenantForLookup).Result;

                _logger.LogInformation($"Subscription lookup for tenant {tenantForLookup} returned {orgDetails.StatusCode}");

                if (orgDetails != null && orgDetails.StatusCode == (int)HttpStatusCode.OK && orgDetails.SingleResult != null)
                {
                    var status = orgDetails.SingleResult.SubscriptionStatus?.ToLower();
                    var isActive = (status == "active" || status == "trialing") && orgDetails.SingleResult.EndDate >= DateTime.UtcNow;

                    HttpContext.Session.SetString("OrganisationSubscriptionStatus", isActive ? "true" : "false");
                }
                else
                {
                    HttpContext.Session.SetString("OrganisationSubscriptionStatus", "false");
                }

                // 5b. Populate the org/user session block
                SetSessionVariables(userResp.SingleResult, identityAlreadyResolved: true, provider: identity.Provider, externalId: identity.ExternalId);
            }
            // 6. Case B: authenticated but no CIUser row yet (first Microsoft/Google sign-in)
            else
            {
                _logger.LogInformation($"No CIUser found for {identity.Email}. Treating as new user.");

                // For Microsoft, the tenant may already have an organization (from a Marketplace purchase)
                if (!string.IsNullOrEmpty(identity.OrganizationKey))
                {
                    var orgByTenant = _subManager.GetOrganizationByTenantId(identity.OrganizationKey).Result;
                    if (orgByTenant != null && orgByTenant.StatusCode == (int)HttpStatusCode.OK && orgByTenant.SingleResult != null)
                    {
                        // Org exists but user record doesn't — send them to Register to join
                        return RedirectToAction("Register", new { Subscribe = 1, IsMarketPlace = true });
                    }
                }

                // Brand-new tenant with no purchase yet → show pricing
                SetSessionVariables(identityAlreadyResolved: true, provider: identity.Provider, externalId: identity.ExternalId);
            }

            return View(_subManager.GetAllSubscriptionPlans().Result);
        }


        private static readonly HashSet<string> FreeEmailProviders =
    new(StringComparer.OrdinalIgnoreCase)
    {
        "gmail.com", "googlemail.com",
        "outlook.com", "hotmail.com", "live.com", "msn.com",
        "yahoo.com", "yahoo.co.uk", "ymail.com",
        "icloud.com", "me.com", "mac.com",
        "proton.me", "protonmail.com",
        "aol.com", "gmx.com", "mail.com", "zoho.com"
    };

        private UserIdentityContext ResolveIdentityContext()
        {
            // ── Provider ─────────────────────────────────────────────────────────
            var tid = User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
            var isMicrosoft = !string.IsNullOrEmpty(tid);

            var provider = isMicrosoft
                ? "Microsoft"
                : User.HasClaim(c => c.Type == "email" || c.Type == System.Security.Claims.ClaimTypes.Email)
                    ? "Google"
                    : "Unknown";

            // ── Email ────────────────────────────────────────────────────────────
            var email = User.FindFirst("preferred_username")?.Value
                     ?? User.FindFirst("email")?.Value
                     ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                     ?? "";

            // ── External id (per-user) ───────────────────────────────────────────
            var externalId = isMicrosoft
                ? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ?? ""
                : User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

            // ── Organization key (per-org) ───────────────────────────────────────
            string? orgKey = null;
            var isConsumer = false;

            if (isMicrosoft)
            {
                orgKey = tid;
            }
            else if (provider == "Google")
            {
                // Google Workspace
                var hd = User.FindFirst("hd")?.Value;
                if (!string.IsNullOrEmpty(hd))
                {
                    orgKey = hd;
                }
                else
                {
                    // Google consumer or custom-domain-via-Google
                    var domain = email.Contains('@') ? email[(email.IndexOf('@') + 1)..] : "";
                    if (FreeEmailProviders.Contains(domain))
                    {
                        isConsumer = true;
                        orgKey = null;         // caller must create / join an org
                    }
                    else if (!string.IsNullOrEmpty(domain))
                    {
                        orgKey = domain;           // custom domain, treat as the org key
                    }
                }
            }

            // ── Name ─────────────────────────────────────────────────────────────
            var (first, last) = User.ResolveName();

            return new UserIdentityContext(
                Provider: provider,
                Email: email,
                ExternalId: externalId,
                OrganizationKey: orgKey ?? "",
                IsConsumerPersonalAccount: isConsumer,
                FirstName: first,
                LastName: last);
        }


        [HttpGet("saas/landing")]
        public async Task<IActionResult> Landing(string token)
        {
            _logger.LogInformation($"SaaS landing accessed at {DateTime.UtcNow}");

            var mpSub = await _msOps.ResolveAsync(token, _adconfig.Value.CITenantId);
            if (mpSub?.Subscription == null)
                return RedirectToAction("Index");

            var purchaser = mpSub.Subscription.Purchaser;
            var status = mpSub.Subscription.SaasSubscriptionStatus;

            // ─── 1. Is this tenant already known to MY system? ───
            var existingOrg = await _subManager.GetOrganizationByTenantId(purchaser.TenantId);
            if (existingOrg?.SingleResult != null)
            {
                _logger.LogInformation($"Returning customer {purchaser.TenantId}");

                // Already signed in → go to dashboard
                if (User.Identity?.IsAuthenticated == true)
                    return RedirectToAction("Dashboard", "Main");

                // Not signed in → send them to sign-in, then dashboard
                return RedirectToAction("SignIn", new { returnUrl = "/Main/Dashboard" });
            }

            // ─── 2. New customer — provision them ───
            _logger.LogInformation($"New customer {purchaser.TenantId}, status={status}");

            var plan = (await _subManager.GetSubscriptionPlanByMarketPlaceId(mpSub.Subscription.PlanId)).SingleResult;

            var res = await _subManager.CreatePendingSubscription(new PendingSubscription
            {
                PlanId = plan.Id,
                SeatsRequested = mpSub.Quantity,   // MSFT sells the full plan
                Provider = "Microsoft",
                ProviderCustomerId = purchaser.TenantId,
                ProviderSubscriptionId = mpSub.Subscription.Id,
                BillingEmail = purchaser.EmailId,
                TrialDays = mpSub.Subscription.IsFreeTrial ? 30 : 0,
                Status = "PaidAwaitingTenant",
                CreatedAt = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(30)
            });

            // ─── 3. Activate ONLY if Microsoft hasn't already ───
            //     (Microsoft-managed activation => status is already "Subscribed")
            if (status == "PendingFulfillmentStart")
            {
                var activated = await _msOps.ActivateAsync(mpSub.Id, _adconfig.Value.CITenantId);
                if (!activated)
                    _logger.LogError($"Activation failed for subscription {mpSub.Id}");
                // Don't hard-fail — a retry job can retry activation within the 30-day window
            }
            else
            {
                _logger.LogInformation($"Skipping Activate — status is already {status} (Microsoft-managed).");
            }

            // ─── 4. Stash session and continue ───
            HttpContext.Session.SetString("PendingId", res.SingleResult.Id.ToString());
            HttpContext.Session.SetString("SubscriptionPlanId", plan.Id.ToString());
            HttpContext.Session.SetString("SeatsPurchased", mpSub.Subscription.Quantity.ToString());
            HttpContext.Session.SetString("MarketplaceSubscriptionId", mpSub.Subscription.Id);
            HttpContext.Session.SetString("UserEmail", purchaser.EmailId);
            HttpContext.Session.SetString("UserName", purchaser.EmailId.Split('@')[0]);
            HttpContext.Session.SetString("TenantId", purchaser.TenantId);

            // ─── 5. Drive them through sign-in → register ───
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("SignIn", new
                {
                    returnUrl = $"/Home/Register?Subscribe={plan.Id}&IsMarketPlace=true"
                });
            }

            return RedirectToAction("Register", new { Subscribe = plan.Id, IsMarketPlace = true });
        }

        //    var purchaser = mpSub.Subscription.Purchaser;

        //    var re = await _subManager.GetSubscriptionPlanByMarketPlaceId(mpSub.PlanId);

        //    var org = await _subManager.UpsertOrganizationFromMarketplace(new OrganisationDTO
        //    {
        //        TenantId = purchaser.TenantId,
        //        AdminEmail = purchaser.EmailId,
        //        AdminName = purchaser.EmailId.Split('@')[0],
        //        Provider = "Microsoft",
        //        CountryId = DefaultCountryId,
        //        PlanId = re.SingleResult.Id
        //    });

        //    // 2. Subscription with full plan seats
        //    var sub = await _subManager.CreateOrUpdateMarketplaceSubscriptionAsync(new()
        //    {
        //        OrganizationId = org.Id,
        //        PlanId = plan.Id,
        //        SeatsPurchased = plan.NumberOfLicences,
        //        ProviderSubscriptionId = resolved.Id,
        //        IsFreeTrial = resolved.Subscription.IsFreeTrial,
        //        TrialDays = resolved.Subscription.IsFreeTrial ? 30 : 0,
        //        StartUtc = resolved.Subscription.Term.StartDate,
        //        EndUtc = resolved.Subscription.Term.EndDate
        //    });

        //    // 3. Store in session and send for sign-in
        //    HttpContext.Session.SetString("MarketplaceOrgId", org.Id.ToString());
        //    HttpContext.Session.SetString("MarketplaceSubId", sub.Id.ToString());
        //    HttpContext.Session.SetString("MarketplaceUserEmail", purchaser.EmailId);

        //    return RedirectToAction("CompleteSignIn");

        //    //HttpContext.Session.SetString("UserEmail", purchaser.EmailId);
        //    //HttpContext.Session.SetString("UserName", purchaser.EmailId.Split('@')[0]?.Replace('.', ' '));
        //    //HttpContext.Session.SetString("TenantId", purchaser.TenantId);
        //    //HttpContext.Session.SetString("MarketplaceSubscriptionId", mpSub.Id);
        //    //HttpContext.Session.SetString("MarketplaceResolvedToken", JsonConvert.SerializeObject(mpSub));

        //    //return RedirectToAction("Register", new { Subscribe = re.SingleResult.Id.ToString(), IsMarketPlace = true });
        //}


        [HttpGet("saas/stripe-success")]
        public async Task<IActionResult> StripeSuccess(string session_id)
        {
            // Stash session id so we can link the pending record
            HttpContext.Session.SetString("StripeCheckoutSessionId", session_id ?? "");
            _logger.LogInformation($"Stripe checkout completed. SessionId={session_id}");

            if (!IsAuthenticated())
                return RedirectToAction("SignIn");   // returns to "/" then back here via session

            // Signed in → link now
            var pending = await _subManager.GetPendingSubscriptionByStripeSession(session_id);
            if (pending == null)
            {
                TempData["Error"] = "We couldn't find your pending subscription.";
                return RedirectToAction("Index");
            }

            if (pending.Status == "AwaitingPayment")
            {
                // Stripe webhook hasn't arrived yet. Show waiting page.
                return View("AwaitingStripeConfirmation");
            }

            return await LinkPendingAndProvision(pending, "Stripe");
        }


        [HttpGet("signin")]
        public IActionResult SignIn(string provider = "OpenIdConnect", string? returnUrl = null)
        {
            var scheme = provider switch
            {
                "Google" => "Google",
                "Corporate" => "CorporateSso",
                _ => "OpenIdConnect"
            };
            return Challenge(
                new AuthenticationProperties { RedirectUri = returnUrl ?? "/" },
                scheme);
        }


        [HttpGet("SignInGoogle")]
        public IActionResult SignInGoogle()
        {
            ClearSessionIdentity();

            return Challenge(
                new AuthenticationProperties { RedirectUri = "/" },
                OpenIdConnectDefaults.AuthenticationScheme);
        }


        public IActionResult SignOut()
        {
            ClearSessionIdentity();

            return SignOut(new AuthenticationProperties { RedirectUri = "/" }, CookieAuthenticationDefaults.AuthenticationScheme); // clears the shared cookie
        }


        [HttpGet("success")]
        public IActionResult Success(string session_id)
        {
            _logger.LogInformation($"Payment request for Organization with tenantId {User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value} has been submitted successfully with Session Id {session_id}");
            return View();
        }


        [HttpGet("failed")]
        public IActionResult Failed()
        {
            _logger.LogInformation($"Payment request for Organization with tenantId {User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value} Failed.");
            return View();
        }


        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl = null)
        {
            // Set the culture cookie
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    HttpOnly = true,
                    IsEssential = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                }
            );

            // Redirect to the return URL or home page
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                // Ensure the return URL includes the culture
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }


        [HttpPost("Checkout")]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(string Subscribe, bool IsMarketPlace = false)
        {
            try
            {
                if (IsAuthenticated())
                {
                    //check if organization has existing mpSub
                    var isSubscribed = HttpContext.Session.GetString("OrganisationSubscriptionStatus");
                    if (isSubscribed == "true")
                        return RedirectToAction("Index");

                    if (!String.IsNullOrEmpty(Subscribe) && !String.IsNullOrWhiteSpace(Subscribe))
                    {
                        //get single mpSub details
                        try
                        {
                            var subs = _subManager.GetSubscriptionPlanById(int.Parse(Subscribe)).Result;
                            var payopts = _payManager.FetchPaymentOptions().Result;
                            var country = _opsManager.FetchOperationalCountry().Result;

                            if (subs.StatusCode != (int)HttpStatusCode.OK || payopts.StatusCode != (int)HttpStatusCode.OK)
                            {
                                _logger.LogInformation($"Invalid Subscription or PaymentOptions Error || subscriptionId - {JsonConvert.SerializeObject(subs)} ||| {JsonConvert.SerializeObject(payopts)}");

                                return RedirectToAction("Index");
                            }

                            var cvm = new CheckoutVM
                            {
                                PaymentProvider = IsMarketPlace == true ? payopts.Result.Where(t => t.Name == "Microsoft").ToList() : payopts.Result.Where(t => t.Name != "Microsoft").ToList(),
                                SubscriptionPlan = subs.SingleResult,
                                Country = country.Result.ToList()
                            };

                            return View(cvm);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Exception at Checkout || subscriptionId - {Subscribe} ||| - {JsonConvert.SerializeObject(ex)}");
                        }
                    }
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("SignIn");
                }
            }
            catch(Exception e)
            {
                return RedirectToAction("Index");
            }

        }

        private async Task<IActionResult> CompleteMicrosoftRegistration(int planId, int seats)
        {
            var pendingIdStr = HttpContext.Session.GetString("PendingId");
            if (!long.TryParse(pendingIdStr, out var pendingId))
                return RedirectToAction("Index");

            var pending = await _subManager.GetPendingSubscription(pendingId);
            if (pending == null) return RedirectToAction("Index");

            var plan = (await _subManager.GetSubscriptionPlanById(planId)).SingleResult;

            if (seats < plan.MinSeats || seats > plan.NumberOfLicences)
            {
                TempData["Error"] = $"Choose between {plan.MinSeats} and {plan.NumberOfLicences} seats.";
                return RedirectToAction("Register", new { Subscribe = planId, IsMarketPlace = true });
            }

            pending.PlanId = planId;
            pending.SeatsRequested = seats;
            await _subManager.UpdatePendingSubscription(pending);

            return await LinkPendingAndProvision(pending, "Microsoft");
        }

        private async Task<IActionResult> LinkPendingAndProvision(PendingSubscription pending, string provider)
        {
            var plan = (await _subManager.GetSubscriptionPlanById(pending.PlanId)).SingleResult;

            // Create organization if not yet created
            int orgId;
            var existingOrg = await _subManager.GetOrganizationByTenantId(HttpContext.Session.GetString("TenantId"));
            if (existingOrg?.SingleResult == null)
            {
                var org = new Organization
                {
                    Name = HttpContext.Session.GetString("CompanyName") ?? pending.BillingEmail,
                    TenantId = HttpContext.Session.GetString("TenantId"),
                    AdminName = HttpContext.Session.GetString("UserName"),
                    AdminEmailAddress = pending.BillingEmail,
                    AdminPhoneNumber = HttpContext.Session.GetString("AdminPhone"),
                    Domain = pending.BillingEmail.Split('@').Last(),
                    CountryId = HttpContext.Session.GetInt32("CountryId") ?? 1,
                    Address = HttpContext.Session.GetString("CompanyAddr"),
                    Provider = provider,
                    IsSubscribed = true,
                    DateCreated = DateTime.UtcNow
                };
                var admin = new CIUser
                {
                    Name = org.AdminName,
                    EmailAddress = org.AdminEmailAddress,
                    Role = "Admin",
                    IsActive = true,
                    DateCreated = DateTime.UtcNow,
                    IdentityProvider = provider
                };
                var trialStart = pending.TrialDays > 0 ? DateTime.UtcNow : (DateTime?)null;
                var trialEnd = pending.TrialDays > 0 ? DateTime.UtcNow.AddDays(pending.TrialDays) : (DateTime?)null;

                var sub = new Subscription
                {
                    SubscriptionPlanId = pending.PlanId,
                    PaymentCustomerId = pending.ProviderCustomerId,
                    PaymentSubscriptionId = pending.ProviderSubscriptionId,
                    Provider = provider,
                    SeatsPurchased = pending.SeatsRequested,
                    SeatsAllocated = 0,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    Status = trialEnd.HasValue ? "TRIALING" : "ACTIVE",
                    TrialStartUtc = trialStart,
                    TrialEndUtc = trialEnd,
                    DateCreated = DateTime.UtcNow
                };
                var res = await _subManager.RegisterOrganizationSubscription(org, admin, sub);
                if (res.StatusCode != 200) { TempData["Error"] = res.Message; return RedirectToAction("Index"); }
                orgId = org.Id;
            }
            else
            {
                orgId = existingOrg.SingleResult.Id;
            }

            await _subManager.MarkPendingSubscriptionLinked(pending.Id, orgId);
            await _subManager.UpdateOrganizationSubscription(orgId, pending.ProviderCustomerId, pending.TrialDays > 0 ? "TRIALING" : "ACTIVE", 0);

            // Refresh session state
            var userResp = await _usrManager.GetUserByEmail(HttpContext.Session.GetString("UserEmail"));
            SetSessionVariables(userResp?.SingleResult, true, provider);

            return RedirectToAction("Dashboard", "Main");
        }

        [HttpGet("Register")]
        public IActionResult Register(int Subscribe, bool IsMarketPlace = false)
        {
            try
            {
                if (Subscribe > 0)
                {
                    //get single mpSub details
                    try
                    {
                        var subs = _subManager.GetSubscriptionPlanById(Subscribe).Result;
                        var payopts = _payManager.FetchPaymentOptions().Result;
                        var country = _opsManager.FetchOperationalCountry().Result;

                        if (subs.StatusCode != (int)HttpStatusCode.OK || payopts.StatusCode != (int)HttpStatusCode.OK)
                        {
                            _logger.LogInformation($"Invalid Subscription or PaymentOptions Error || subscriptionId - {JsonConvert.SerializeObject(subs)} ||| {JsonConvert.SerializeObject(payopts)}");

                            return RedirectToAction("Index");
                        }

                        var cvm = new CheckoutVM
                        {
                            PaymentProvider = IsMarketPlace == true ? payopts.Result.Where(t => t.Name == "Microsoft").ToList() : payopts.Result.Where(t => t.Name != "Microsoft").ToList(),
                            SubscriptionPlan = subs.SingleResult,
                            Country = country.Result.ToList(),
                            PendingId = long.Parse(HttpContext.Session.GetString("PendingId"))
                        };

                        return View(cvm);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Exception at Register || subscriptionId - {Subscribe} ||| - {JsonConvert.SerializeObject(ex)}");
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception at Register2 || subscriptionId - {Subscribe} ||| - {JsonConvert.SerializeObject(e)}");

                return RedirectToAction("Index");
            }
        }


        [HttpPost("StartSubscription")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartSubscription(int planId, int seats, string provider)
        {
            if (!IsAuthenticated()) return RedirectToAction("SignIn");

            var tenantId = HttpContext.Session.GetString("TenantId");
            var email = HttpContext.Session.GetString("UserEmail");

            var plan = (await _subManager.GetSubscriptionPlanById(planId)).SingleResult;
            if (plan == null) return RedirectToAction("Index");

            if (seats < plan.MinSeats || seats > plan.NumberOfLicences)
            {
                TempData["Error"] = $"Choose between {plan.MinSeats} and {plan.NumberOfLicences} seats for {plan.Name}.";
                return RedirectToAction("Register", new { Subscribe = planId });
            }

            // Existing organization?
            var orgResp = await _subManager.GetOrganizationByTenantId(tenantId);
            int? orgId = orgResp?.SingleResult?.Id;

            if (provider == "stripe")
            {
                var trialDays = plan.FreeTrialDuration;

                var res = await _subManager.CreatePendingSubscription(new PendingSubscription
                {
                    OrganizationId = orgId,
                    PlanId = planId,
                    SeatsRequested = seats,
                    Provider = "Stripe",
                    BillingEmail = email,
                    TrialDays = trialDays,
                    Status = "AwaitingPayment",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAtUtc = DateTime.UtcNow.AddHours(24)
                });

                var url = _strPay.BuildPaymentLinkUrl(plan.StripePaymentLinkUrl, res.SingleResult.Id, email, seats);
                return Redirect(url);
            }

            if (provider == "microsoft")
            {
                // Only valid if the tenant already has a Microsoft-created PendingSubscription
                var mpSubId = HttpContext.Session.GetString("MarketplaceSubscriptionId");
                if (string.IsNullOrEmpty(mpSubId))
                {
                    TempData["Error"] = "Please purchase the Microsoft Marketplace offer first.";
                    return RedirectToAction("Index");
                }
                // Set seats + plan on the session-stored subscription; complete registration
                return await CompleteMicrosoftRegistration(planId, seats);
            }

            return RedirectToAction("Index");
        }


        [HttpPost("RegisterPayment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPayment()
        {
            try
            {
                // 1. Pending subscription is the source of truth for plan + seats
                var pendingIdStr = HttpContext.Session.GetString("PendingId");
                if (!long.TryParse(pendingIdStr, out var pendingId))
                    return RedirectToAction("Index");

                var pending = await _subManager.GetPendingSubscription(pendingId);
                if (pending == null)
                    return RedirectToAction("Index");

                // 2. Reject duplicates (an org already active)
                var tenantId = HttpContext.Session.GetString("TenantId") ?? "";
                var existingSub = await _subManager.GetOrganizationSubscription(tenantId);
                if (existingSub?.SingleResult?.EndDate > DateTime.UtcNow)
                {
                    TempData["Error"] = "Your organisation already has an active subscription.";
                    return RedirectToAction("Index");
                }

                // 3. Capture additional info the Marketplace doesn't give us
                HttpContext.Session.SetString("CompanyName", Request.Form["companyName"]);
                HttpContext.Session.SetString("CompanyAddr", Request.Form["address"]);
                HttpContext.Session.SetString("AdminPhone", Request.Form["phone"]);
                HttpContext.Session.SetString("AdminName", Request.Form["firstName"]);
                HttpContext.Session.SetInt32("CountryId", int.Parse(Request.Form["country"]));

                // 4. No need to touch pending.SeatsRequested — it already
                //    contains the seats the customer paid for on Marketplace.

                // 5. Provision
                return await LinkPendingAndProvision(pending, "Microsoft");
            }
            catch (Exception ex)
            {
                _logger.LogError($"RegisterPayment error ||| {JsonConvert.SerializeObject(ex)}");
                TempData["Error"] = "We could not complete your registration. Please try again.";
                return RedirectToAction("Index");
            }
        }

        //[HttpPost("RegisterPayment")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> RegisterPayment()
        //{
        //    ResponseHandler<SubscriptionPlan> subscription = null;
        //    try
        //    {
        //        string domain = Request.Form["adminEmail"].ToString().Split('@')[1];
        //        int subscriptionPlanId = int.Parse(HttpContext.Session.GetString("SubscriptionPlanId"));
        //        int seatsPurchased = int.Parse(HttpContext.Session.GetString("SeatsPurchased"));

        //        subscription = await _subManager.GetSubscriptionPlanById(subscriptionPlanId);

        //        if (subscription == null)
        //        {
        //            return RedirectToAction("Index");
        //        }

        //        //check if organization has an existing active mpSub
        //        //this is to deter any other member of an organization from creating multiple subscriptions for the same organization
        //        var orgSubscription = _subManager.GetOrganizationSubscription(HttpContext.Session.GetString("TenantId").ToString()).Result;

        //        if (orgSubscription.SingleResult != null)
        //        {
        //            if (orgSubscription.SingleResult.EndDate > DateTime.Now)
        //            {
        //                return View("Checkout", new CheckoutVM
        //                {
        //                    StatusCode = (int)HttpStatusCode.ExpectationFailed,
        //                    Message = $"Organisation - {Request.Form["companyName"]} - has existing mpSub.",
        //                    SubscriptionPlan = subscription.SingleResult,
        //                    PaymentProvider = _payManager.FetchPaymentOptions().Result.Result.ToList(),
        //                    Country = _opsManager.FetchOperationalCountry().Result.Result.ToList()
        //                });
        //            }
        //        }

        //        //build Organisation details
        //        var org = new Organization
        //        {
        //            Name = Request.Form["companyName"],
        //            TenantId = HttpContext.Session.GetString("TenantId").ToString(),
        //            Address = Request.Form["address"],
        //            AdminName = Request.Form["firstName"],
        //            AdminEmailAddress = Request.Form["adminEmail"],
        //            AdminPhoneNumber = Request.Form["phone"],
        //            CountryId = int.Parse(Request.Form["country"]),
        //            Provider = "Microsoft",
        //            Domain = domain,
        //            DateCreated = DateTime.UtcNow
        //        };

        //        //build user details
        //        var usr = new CIUser
        //        {
        //            Name = Request.Form["firstName"],
        //            EmailAddress = Request.Form["adminEmail"],
        //            Role = Shared.Enumerations.Role.Admin.ToString(),
        //            DateCreated = DateTime.UtcNow
        //        };


        //        var selectedDuration = 1; // int.Parse(Request.Form["subscriptionDuration"]);

        //        //build mpSub details
        //        var sub = new Subscription
        //        {
        //            SubscriptionPlanId = subscriptionPlanId,
        //            PaymentSubscriptionId = HttpContext.Session.GetString("MarketplaceSubscriptionId").ToString(),
        //            StartDate = subscription.SingleResult.FreeTrialDuration > 0 ? DateTime.UtcNow.AddDays(subscription.SingleResult.FreeTrialDuration) : DateTime.UtcNow,
        //            EndDate = subscription.SingleResult.FreeTrialDuration > 0 ? DateTime.UtcNow.AddDays(subscription.SingleResult.FreeTrialDuration).AddYears(selectedDuration) : DateTime.UtcNow.AddYears(selectedDuration),
        //            DateCreated = DateTime.UtcNow,
        //            Status = SubscriptionStatus.ACTIVE.ToString(),
        //            SeatsPurchased = seatsPurchased,
        //            SeatsAllocated = seatsPurchased
        //        };

        //        var resp = _subManager.RegisterOrganizationSubscription(org, usr, sub).Result;

        //        if (resp.StatusCode != (int)HttpStatusCode.OK)
        //        {
        //            _logger.LogInformation($"Unable to Register Organization {org.Name}");

        //            return View("Checkout", new CheckoutVM
        //            {
        //                StatusCode = (int)HttpStatusCode.ExpectationFailed,
        //                Message = $"Unable to Register Organisation  {org.Name} ||| {resp.Message}",
        //                SubscriptionPlan = subscription.SingleResult,
        //                PaymentProvider = _payManager.FetchPaymentOptions().Result.Result.ToList(),
        //                Country = _opsManager.FetchOperationalCountry().Result.Result.ToList()
        //            });
        //        }

        //        //get user's detail
        //        var user = _usrManager.GetUserByEmail(HttpContext.Session.GetString("UserEmail").ToString()).Result;
        //        SetSessionVariables(user.SingleResult, true);

        //        //Redirect to failed mpSub page
        //        return RedirectToAction("Index", "Home");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Exception at RegisterPayment || - {JsonConvert.SerializeObject(ex)}");

        //        return View("Register", new CheckoutVM
        //        {
        //            StatusCode = (int)HttpStatusCode.InternalServerError,
        //            Message = $"Unable to Register Organisation ",
        //            SubscriptionPlan = subscription.SingleResult,
        //            PaymentProvider = _payManager.FetchPaymentOptions().Result.Result.ToList(),
        //            Country = _opsManager.FetchOperationalCountry().Result.Result.ToList()
        //        });
        //    }
        //}


        [HttpPost("MakePayment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakePayment()
        {
            ResponseHandler<SubscriptionPlan> subscription = null;
            try
            {
                string domain = Request.Form["adminEmail"].ToString().Split('@')[1];

                //check if organization has an existing active mpSub
                //this is to deter any other member of an organization from creating multiple subscriptions for the same organization
                var orgSubscription = _subManager.GetOrganizationSubscription(HttpContext.Session.GetString("TenantId").ToString()).Result;

                if (orgSubscription.SingleResult != null)
                {
                    if (orgSubscription.SingleResult.EndDate > DateTime.Now)
                    {
                        return View("Checkout", new CheckoutVM
                        {
                            StatusCode = (int)HttpStatusCode.ExpectationFailed,
                            Message = $"Organisation - {Request.Form["companyName"]} - has existing mpSub.",
                            SubscriptionPlan = _subManager.GetSubscriptionPlanById(int.Parse(Request.Form["subscriptionId"])).Result?.SingleResult,
                            PaymentProvider = _payManager.FetchPaymentOptions().Result.Result.ToList(),
                            Country = _opsManager.FetchOperationalCountry().Result.Result.ToList()
                        });
                    }
                }

                var provider = Request.Form["paymentMethod"].ToString().ToLower();

                //build Organisation details
                var org = new Organization
                {
                    Name = Request.Form["companyName"],
                    TenantId = HttpContext.Session.GetString("TenantId").ToString(),
                    Address = Request.Form["address"],
                    AdminName = Request.Form["firstName"],
                    AdminEmailAddress = Request.Form["adminEmail"],
                    AdminPhoneNumber = Request.Form["phone"],
                    CountryId = int.Parse(Request.Form["country"]),
                    Provider = provider,
                    Domain = domain,
                    DateCreated = DateTime.UtcNow
                };

                //build user details
                var usr = new CIUser
                {
                    Name = Request.Form["firstName"],
                    EmailAddress = Request.Form["adminEmail"],
                    Role = Shared.Enumerations.Role.Admin.ToString(),
                    DateCreated = DateTime.UtcNow,
                    IdentityProvider = provider                    
                };

                //get mpSub Details
                subscription = _subManager.GetSubscriptionPlanById(int.Parse(Request.Form["subscriptionId"])).Result;

                if (subscription == null || subscription?.SingleResult == null)
                {
                    return RedirectToAction("Index");
                }

                var selectedDuration = int.Parse(Request.Form["subscriptionDuration"]);

                //build mpSub details
                var sub = new Subscription
                {
                    SubscriptionPlanId = int.Parse(Request.Form["subscriptionId"]),
                    PaymentSubscriptionId = provider == "microsoft" ? HttpContext.Session.GetString("MarketplaceSubscriptionId").ToString() : null,
                    StartDate = subscription.SingleResult.FreeTrialDuration > 0 ? DateTime.UtcNow.AddDays(subscription.SingleResult.FreeTrialDuration) : DateTime.UtcNow,
                    EndDate = subscription.SingleResult.FreeTrialDuration > 0 ? DateTime.UtcNow.AddDays(subscription.SingleResult.FreeTrialDuration).AddYears(selectedDuration) : DateTime.UtcNow.AddYears(selectedDuration),
                    DateCreated = DateTime.UtcNow,
                    Provider = provider,
                    SeatsPurchased = subscription.SingleResult.NumberOfLicences,
                    Status = "PENDING"
                };

                var resp = _subManager.RegisterOrganizationSubscription(org, usr, sub).Result;

                if (resp.StatusCode != (int)HttpStatusCode.OK)
                {
                    _logger.LogInformation($"Unable to Register Organization {org.Name}");

                    return View("Checkout", new CheckoutVM
                    {
                        StatusCode = (int)HttpStatusCode.ExpectationFailed,
                        Message = $"Unable to Register Organisation  {org.Name} ||| {resp.Message}",
                        SubscriptionPlan = subscription.SingleResult,
                        PaymentProvider = _payManager.FetchPaymentOptions().Result.Result.ToList(),
                        Country = _opsManager.FetchOperationalCountry().Result.Result.ToList()
                    });
                }

                //get user's detail
                var user = _usrManager.GetUserByEmail(HttpContext.Session.GetString("UserEmail").ToString()).Result;
                SetSessionVariables(user.SingleResult, true, provider);

                if (provider == "stripe")
                {
                    if (!String.IsNullOrEmpty(subscription.SingleResult.StripePaymentLinkUrl))
                    {
                        var res = await _subManager.CreatePendingSubscription(new PendingSubscription
                        {
                            OrganizationId = resp.SingleResult.Id,
                            PlanId = sub.SubscriptionPlanId,
                            SeatsRequested = subscription.SingleResult.NumberOfLicences,
                            Provider = "Stripe",
                            BillingEmail = usr.EmailAddress,
                            TrialDays = subscription.SingleResult.FreeTrialDuration,
                            Status = "AwaitingPayment",
                            CreatedAt = DateTime.UtcNow,
                            ExpiresAtUtc = DateTime.UtcNow.AddHours(24)
                        });

                        var url = _strPay.BuildPaymentLinkUrl(subscription.SingleResult.StripePaymentLinkUrl, res.SingleResult.Id, usr.EmailAddress, subscription.SingleResult.NumberOfLicences);
                        return Redirect(url);
                    }
                    else
                    {
                        //create organization as stripe customer and get customer id
                        var orgi = _usrManager.GetOrganizationByTenant(User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value).Result;
                        _logger.LogInformation($"About to CreateStripeCustomer for {orgi.SingleResult.Name} with {orgi.SingleResult.AdminEmailAddress} and Id {orgi.SingleResult.Id}");
                        var res = _strPay.CreateStripeCustomer(orgi.SingleResult.AdminEmailAddress, orgi.SingleResult.Id.ToString()).Result;
                        _logger.LogInformation($"CreateStripeCustomer response for {orgi.SingleResult.Name} - {JsonConvert.SerializeObject(res)}");
                        if (!String.IsNullOrEmpty(res.Id))
                        {
                            //update user's mpSub with their stripecustomerId as reference for future payments
                            await _subManager.UpdateOrganizationSubscription(orgi.SingleResult.Id, res.Id, SubscriptionStatus.INITIATED.ToString(), user.SingleResult.Id);
                        }

                        _logger.LogInformation($"About to checkout Organization {orgi.SingleResult.Name}");
                        var chkres = _strPay.CreateCheckout(orgi.SingleResult.Id.ToString(), res.Id, subscription.SingleResult.PriceId, selectedDuration, subscription.SingleResult.FreeTrialDuration).Result;
                        _logger.LogInformation($"CreateCheckout response for {orgi.SingleResult.Name} - {JsonConvert.SerializeObject(chkres)}");
                        if (!string.IsNullOrEmpty(chkres))
                        {
                            return Redirect(chkres);
                        }

                        //Redirect to failed mpSub page
                        return RedirectToAction("Index", "Home");
                    }                        
                }
                else
                {
                    _logger.LogInformation($"should not manually activate a subscription till I know what it is for. Microsoft should handle this ||| MarketplaceSubscriptionId - {HttpContext.Session.GetString("MarketplaceSubscriptionId").ToString()}, Tenant ||| {_adconfig.Value.CITenantId}");

                    //Redirect to failed mpSub page
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at MakePayment || - {JsonConvert.SerializeObject(ex)}");

                return View("Checkout", new CheckoutVM
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = $"Unable to Register Organisation ",
                    SubscriptionPlan = subscription.SingleResult,
                    PaymentProvider = _payManager.FetchPaymentOptions().Result.Result.ToList(),
                    Country = _opsManager.FetchOperationalCountry().Result.Result.ToList()
                });
            }
        }


        [HttpPost("Contact")]
        [ValidateAntiForgeryToken]
        public IActionResult Contact()
        {
            try
            {
                if (!Utils.IsValidEmail(Request.Form["cusEmail"].ToString()))
                {
                    TempData["message"] = "Kindly provide a valid email address";
                    return RedirectToAction("Index");
                }

                //build email object
                var org = new EmailDTO
                {
                    Name = Request.Form["cusName"],
                    Email = Request.Form["cusEmail"],
                    Message = Request.Form["cusMessage"],
                    Subject = Request.Form["cusSubject"]
                };


                //send email to CITracker
                var rply = new List<ReplyTo> {  new ReplyTo
                    {
                        EmailAddress = org.Email,
                        Name = org.Name
                    }
                };

                _mail.sendEmail(_config.Value.ContactEmail, "CITracker Contact Form", "CITracker", _mail.PopulateContactBody(org), rply, true);

                _mail.sendEmail(org.Email, org.Subject, "CITracker", _mail.PopulateContactReceiptBody(org));

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at Contact || - {JsonConvert.SerializeObject(ex)}");

                TempData["message"] = $"{JsonConvert.SerializeObject(ex)}";

                return RedirectToAction("Index");
            }
        }


        private bool IsAuthenticated()
        {
            if (User.Identity.IsAuthenticated)
            {
                //set user Email first if user email is null
                if (String.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
                {
                    HttpContext.Session.SetString("UserEmail", User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value ?? "");

                    HttpContext.Session.SetString("UserName", User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "");
                }

                return true;
            }

            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail"));
        }


        //private void SetSessionVariables(CIUserDTO user = null, bool IsMarketPlace = false)
        //{
        //    if (!IsMarketPlace)
        //    {
        //        HttpContext.Session.SetString("UserEmail", User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value ?? "");
        //        HttpContext.Session.SetString("UserName", User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "");
        //        HttpContext.Session.SetString("TenantId", User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value ?? "");
        //        HttpContext.Session.SetString("ObjectId", User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ?? "");
        //    }

        //    if (user != null)
        //    {
        //        HttpContext.Session.SetString("UserRole", user.Role);
        //        HttpContext.Session.SetString("Domain", user.OrganizationDomain ?? "");
        //        HttpContext.Session.SetString("OrganizationId", user.OrganizationId.ToString());
        //        HttpContext.Session.SetString("UserId", user.Id.ToString());
        //    }
        //    else
        //    {
        //        HttpContext.Session.SetString("UserRole", "");
        //        HttpContext.Session.SetString("Domain", "");
        //        HttpContext.Session.SetString("OrganizationId", "");
        //        HttpContext.Session.SetString("UserId", "");

        //        HttpContext.Session.SetString("OrganisationSubscriptionStatus", "false");
        //    }
        //}

        private void SetSessionVariables(CIUserDTO user = null, bool identityAlreadyResolved = false, string provider = null, string externalId = null, string tenantHint = null)
        {
            // -------------------------------------------------------------
            // 1. Identity block
            // -------------------------------------------------------------
            if (!identityAlreadyResolved)
            {
                // Prefer values already written to session by the sign-in callback
                // (Google / Local / SAML all set these in OnCreatingTicket or the SignUp action).
                var sessionEmail = HttpContext.Session.GetString("UserEmail");
                var sessionName = HttpContext.Session.GetString("UserName");
                var sessionTenant = HttpContext.Session.GetString("TenantId");
                var sessionObjectId = HttpContext.Session.GetString("ObjectId");
                var sessionProvider = HttpContext.Session.GetString("IdentityProvider");
                var sessionExtId = HttpContext.Session.GetString("ExternalId");

                // Fall back to Microsoft claims only when we truly have an authenticated
                // Microsoft principal and the session has nothing yet.
                var isMicrosoft = User?.Identity?.IsAuthenticated == true && (sessionProvider == null || sessionProvider == "Microsoft");

                var email = sessionEmail ?? (isMicrosoft ? User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value : null);
                var name = sessionName ?? (isMicrosoft ? User.Claims.FirstOrDefault(c => c.Type == "name")?.Value : null);
                var tenant = sessionTenant ?? (isMicrosoft ? User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value : null);
                var objectId = sessionObjectId ?? (isMicrosoft ? User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value : null);

                HttpContext.Session.SetString("UserEmail", email ?? "");
                HttpContext.Session.SetString("UserName", name ?? "");
                HttpContext.Session.SetString("TenantId", tenant ?? "");
                HttpContext.Session.SetString("ObjectId", objectId ?? "");
                HttpContext.Session.SetString("IdentityProvider", provider ?? sessionProvider ?? (isMicrosoft ? "Microsoft" : ""));
                HttpContext.Session.SetString("ExternalId", externalId ?? sessionExtId ?? objectId ?? "");
            }
            else
            {
                // Landing / Stripe-success already populated identity keys.
                // Only fill the provider/external-id fields if they were passed in.
                if (!string.IsNullOrEmpty(provider))
                    HttpContext.Session.SetString("IdentityProvider", provider);
                if (!string.IsNullOrEmpty(externalId))
                    HttpContext.Session.SetString("ExternalId", externalId);
                if (!string.IsNullOrEmpty(tenantHint) &&
                    string.IsNullOrEmpty(HttpContext.Session.GetString("TenantId")))
                    HttpContext.Session.SetString("TenantId", tenantHint);
            }

            // -------------------------------------------------------------
            // 2. Organization / user block
            // -------------------------------------------------------------
            if (user != null)
            {
                HttpContext.Session.SetString("UserRole", user.Role ?? "");
                HttpContext.Session.SetString("Domain", user.OrganizationDomain ?? "");
                HttpContext.Session.SetString("OrganizationId", user.OrganizationId.ToString());
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                // Do not touch OrganisationSubscriptionStatus here — Index() sets it
                // based on the live subscription row.
            }
            else
            {
                HttpContext.Session.SetString("UserRole", "");
                HttpContext.Session.SetString("Domain", "");
                HttpContext.Session.SetString("OrganizationId", "");
                HttpContext.Session.SetString("UserId", "");
                HttpContext.Session.SetString("OrganisationSubscriptionStatus", "false");
            }
        }

        private void ClearSessionIdentity()
        {
            HttpContext.Session.SetString("UserEmail", "");
            HttpContext.Session.SetString("UserName", "");
            HttpContext.Session.SetString("Domain", "");
            HttpContext.Session.SetString("TenantId", "");
            HttpContext.Session.SetString("ObjectId", "");
            HttpContext.Session.SetString("IdentityProvider", "");
            HttpContext.Session.SetString("ExternalId", "");
            HttpContext.Session.SetString("OrganisationSubscriptionStatus", "false");
        }
    }
}
