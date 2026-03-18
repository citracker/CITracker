using CITracker.Helpers;
using Datalayer.Interfaces;
using Infastructure.Implementation;
using Infastructure.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using Shared;
using Shared.DTO;
using Shared.Enumerations;
using Shared.Models;
using Shared.Utilities;
using Shared.ViewModels;
using Stripe;
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
        private readonly Mailer _mail;

        public HomeController(ILogger<HomeController> logger, IOptions<KeyValues> config, ISubscriptionManager subManager, IPaymentManager payManager, IOperationManager opsManager, IUserManager usrManager, Mailer mail, IMicrosoftOperations msOps, ITokenAcquisition tokenAcquisition, IStripePayment strPay)
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
        }


        [HttpGet("")]
        public IActionResult Index()
        {
            //check if user is Authenticated
            if (!IsAuthenticated())
                return View(_subManager.GetAllSubscriptionPlans().Result);


            _logger.LogInformation($"User {User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value} is authenticated");

            _logger.LogInformation($"User {User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value} session variables not set. Fetching user org details and setting session variables.");
                
            //check if tenant of logged in user has existing subscription
            var orgdetails = _subManager.GetOrganizationSubscription(User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value).Result;

            _logger.LogInformation($"Organization details for tenant {User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value} fetched with status code {orgdetails.StatusCode}");

            //organization details not found, set session variables to default and return
            if (orgdetails.StatusCode != (int)HttpStatusCode.OK)
            {
                SetSessionVariables();
                return View(_subManager.GetAllSubscriptionPlans().Result);
            }


            if ((orgdetails.SingleResult.SubscriptionStatus?.ToLower() == "active" || orgdetails.SingleResult.SubscriptionStatus?.ToLower() == "trialing") && orgdetails.SingleResult.EndDate >= DateTime.UtcNow)
            {
                HttpContext.Session.SetString("OrganisationSubscriptionStatus", "true");
            }
            else
            {
                HttpContext.Session.SetString("OrganisationSubscriptionStatus", "false");
            }

            //get user's detail
            var user = _usrManager.GetUserByEmail(User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value).Result;
            if (user.StatusCode != 404)
            {
                SetSessionVariables(user.SingleResult);
            }
            else
            {
                SetSessionVariables();
            }

            return View(_subManager.GetAllSubscriptionPlans().Result);
        }


        public IActionResult SignIn()
        {
            HttpContext.Session.SetString("UserEmail", "");
            HttpContext.Session.SetString("UserName", "");
            HttpContext.Session.SetString("Domain", "");
            HttpContext.Session.SetString("TenantId", "");
            HttpContext.Session.SetString("ObjectId", "");
            HttpContext.Session.SetString("OrganisationSubscriptionStatus", "false");
            
            return Challenge(            
                new AuthenticationProperties { RedirectUri = "/" },            
                OpenIdConnectDefaults.AuthenticationScheme);
        }


        public IActionResult SignOut()
        {
            HttpContext.Session.SetString("UserEmail", "");
            HttpContext.Session.SetString("UserName", "");
            HttpContext.Session.SetString("Domain", "");
            HttpContext.Session.SetString("TenantId", "");
            HttpContext.Session.SetString("ObjectId", "");
            HttpContext.Session.SetString("OrganisationSubscriptionStatus", "false");

            return SignOut(
                new AuthenticationProperties { RedirectUri = "/" },
                OpenIdConnectDefaults.AuthenticationScheme
            );
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
        public IActionResult Checkout(string Subscribe)
        {
            try
            {
                if (IsAuthenticated())
                {
                    //check if organization has existing subscription
                    var isSubscribed = HttpContext.Session.GetString("OrganisationSubscriptionStatus");
                    if (isSubscribed == "true")
                        return RedirectToAction("Index");

                    if (!String.IsNullOrEmpty(Subscribe) && !String.IsNullOrWhiteSpace(Subscribe))
                    {
                        //get single subscription details
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
                                PaymentProvider = payopts.Result.ToList(),
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
        

        [HttpPost("MakePayment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakePayment()
        {
            ResponseHandler<SubscriptionPlan> subscription = null;
            try
            {
                //var accessToken = _tokenAcquisition.GetAccessTokenForUserAsync(new[] { "Organization.Read.All" }).Result;

                //string domain = _msOps.GetOrganizationDomain(accessToken).Result;

                string domain = Request.Form["adminEmail"].ToString().Split('@')[1];

                //check if organization has an existing active subscription
                //this is to deter any other member of an organization from creating multiple subscriptions for the same organization
                var orgSubscription = _subManager.GetOrganizationSubscription(User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value).Result;

                if(orgSubscription.SingleResult != null)
                {
                    if(orgSubscription.SingleResult.EndDate > DateTime.Now)
                    {
                        return View("Checkout", new CheckoutVM
                        {
                            StatusCode = (int)HttpStatusCode.ExpectationFailed,
                            Message = $"Organisation - {Request.Form["companyName"]} - has existing subscription.",
                            SubscriptionPlan = _subManager.GetSubscriptionPlanById(int.Parse(Request.Form["subscriptionId"])).Result?.SingleResult,
                            PaymentProvider = _payManager.FetchPaymentOptions().Result.Result.ToList(),
                            Country = _opsManager.FetchOperationalCountry().Result.Result.ToList()
                        });
                    }
                }

                //build Organisation details
                var org = new Organization
                {
                    Name = Request.Form["companyName"],
                    TenantId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value ?? "",
                    Address = Request.Form["address"],
                    AdminName = Request.Form["firstName"],
                    AdminEmailAddress = Request.Form["adminEmail"],
                    AdminPhoneNumber = Request.Form["phone"],
                    CountryId = int.Parse(Request.Form["country"]),
                    Provider = "Microsoft",
                    Domain = domain,
                    DateCreated = DateTime.UtcNow                    
                };

                //build user details
                var usr = new CIUser
                {
                    Name = Request.Form["firstName"],
                    EmailAddress = Request.Form["adminEmail"],
                    Role = Shared.Enumerations.Role.Admin.ToString(),
                    DateCreated = DateTime.UtcNow                    
                };

                //get subscription Details
                subscription = _subManager.GetSubscriptionPlanById(int.Parse(Request.Form["subscriptionId"])).Result;

                if (subscription == null || subscription?.SingleResult == null)
                {
                    return RedirectToAction("Index");
                }

                var selectedDuration = int.Parse(Request.Form["subscriptionDuration"]);

                //build subscription details
                var sub = new Subscription
                {
                    SubscriptionPlanId = int.Parse(Request.Form["subscriptionId"]),
                    StartDate = subscription.SingleResult.FreeTrialDuration > 0 ? DateTime.UtcNow.AddDays(subscription.SingleResult.FreeTrialDuration) : DateTime.UtcNow,
                    EndDate = subscription.SingleResult.FreeTrialDuration > 0 ? DateTime.UtcNow.AddDays(subscription.SingleResult.FreeTrialDuration).AddYears(selectedDuration) : DateTime.UtcNow.AddYears(selectedDuration),
                    DateCreated = DateTime.UtcNow                    
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

                //HttpContext.Session.SetString("OrganisationSubscriptionStatus", "true");

                //get user's detail
                var user = _usrManager.GetUserByEmail(User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value).Result;
                SetSessionVariables(user.SingleResult);


                //create organization as stripe customer and get customer id
                var orgi = _usrManager.GetOrganizationByTenant(User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value).Result;
                _logger.LogInformation($"About to CreateStripeCustomer for {orgi.SingleResult.Name} with {orgi.SingleResult.AdminEmailAddress} and Id {orgi.SingleResult.Id}");
                var res = _strPay.CreateStripeCustomer(orgi.SingleResult.AdminEmailAddress, orgi.SingleResult.Id.ToString()).Result;
                _logger.LogInformation($"CreateStripeCustomer response for {orgi.SingleResult.Name} - {JsonConvert.SerializeObject(res)}");
                if (!String.IsNullOrEmpty(res.Id))
                {
                    //update user's subscription with their stripecustomerId as reference for future payments
                    await _subManager.UpdateOrganizationSubscription(orgi.SingleResult.Id, res.Id, SubscriptionStatus.INITIATED.ToString(), user.SingleResult.Id);
                }

                _logger.LogInformation($"About to checkout Organization {orgi.SingleResult.Name}");
                var chkres = _strPay.CreateCheckout(orgi.SingleResult.Id.ToString(), res.Id, subscription.SingleResult.PriceId, selectedDuration).Result;
                _logger.LogInformation($"CreateCheckout response for {orgi.SingleResult.Name} - {JsonConvert.SerializeObject(chkres)}");
                if (!string.IsNullOrEmpty(chkres))
                {
                    return Redirect(chkres);
                }

                //Redirect to failed subscription page
                return RedirectToAction("Index", "Home");
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

                return Json(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message
                });
            }
        }


        private bool IsAuthenticated()
        {
            if(User.Identity.IsAuthenticated)
            {
                //set user Email first if user email is null
                if (String.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
                {
                    HttpContext.Session.SetString("UserEmail", User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value ?? "");

                    HttpContext.Session.SetString("UserName", User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "");
                }

                return true;
            }
            return false;
        }


        private void SetSessionVariables(CIUserDTO user = null)
        {
            HttpContext.Session.SetString("UserEmail", User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value ?? "");
            HttpContext.Session.SetString("UserName", User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "");
            HttpContext.Session.SetString("TenantId", User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value ?? "");
            HttpContext.Session.SetString("ObjectId", User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ?? "");

            if (user != null)
            {
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("Domain", user.OrganizationDomain ?? "");
                HttpContext.Session.SetString("OrganizationId", user.OrganizationId.ToString());
                HttpContext.Session.SetString("UserId", user.Id.ToString());
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
    }
}
