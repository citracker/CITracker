using CITracker.Helpers;
using Datalayer.Interfaces;
using Infastructure.Implementation;
using Infastructure.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using Shared;
using Shared.DTO;
using Shared.Models;
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
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IOptions<KeyValues> _config;
        private readonly Mailer _mail;

        public HomeController(ILogger<HomeController> logger, IOptions<KeyValues> config, ISubscriptionManager subManager, IPaymentManager payManager, IOperationManager opsManager, IUserManager usrManager, Mailer mail, IMicrosoftOperations msOps, ITokenAcquisition tokenAcquisition)
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
        }


        [HttpGet("")]
        public IActionResult Index()
        {
            //check if user is Authenticated
            if (IsAuthenticated())
            {
                _logger.LogInformation($"User {User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value} is authenticated");

                //check if organization's domain is not in session
                if (String.IsNullOrEmpty(HttpContext.Session.GetString("Domain")))
                {
                    _logger.LogInformation($"User {User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value} session variables not set. Fetching user details and setting session variables.");

                    //check if tenant of logged in user has existing subscription
                    var orgdetails = _subManager.GetOrganizationByTenantId(User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value).Result;

                    _logger.LogInformation($"Organization details for tenant {User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value} fetched with status code {orgdetails.StatusCode}");

                    if (orgdetails.StatusCode == (int)HttpStatusCode.OK)
                    {
                        if (orgdetails.SingleResult.IsSubscribed)
                        {
                            HttpContext.Session.SetString("OrganisationSubscriptionStatus", "true");
                        }

                        //get user's detail
                        var user = _usrManager.GetUserByEmail(User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value).Result;
                        if(user.StatusCode != 404)
                        {
                            SetSessionVariables(user.SingleResult);
                        }
                    }
                }
            }

            var re = _subManager.GetAllSubscriptionPlans().Result;

            return View(re);
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


        [HttpPost("Checkout")]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(string Subscribe)
        {
            try
            {
                if (IsAuthenticated())
                {
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
        public IActionResult MakePayment()
        {
            ResponseHandler<SubscriptionPlan> subscription = null;
            try
            {
                var accessToken = _tokenAcquisition.GetAccessTokenForUserAsync(new[] { "Organization.Read.All" }).Result;

                string domain = _msOps.GetOrganizationDomain(accessToken).Result;

                //check if organization has existing subscription
                var orgSubscription = _subManager.GetOrganizationSubscription(User.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value).Result;

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

                //build payment details
                var pay = new Payment
                {
                    Amount = CalculateSubscriptionAmount(subscription.SingleResult, selectedDuration),
                    Provider = Request.Form["paymentMethod"],
                    Reference = $"testreference-{DateTime.UtcNow.ToString("yyMMddhhmmss.fff")}",
                    SubscriptionPlanId = int.Parse(Request.Form["subscriptionId"]),
                    DateCreated = DateTime.UtcNow                    
                };

                //build subscription details
                var sub = new Subscription
                {
                    SubscriptionPlanId = int.Parse(Request.Form["subscriptionId"]),
                    StartDate = subscription.SingleResult.FreeTrialDuration > 0 ? DateTime.UtcNow.AddDays(subscription.SingleResult.FreeTrialDuration) : DateTime.UtcNow,
                    EndDate = subscription.SingleResult.FreeTrialDuration > 0 ? DateTime.UtcNow.AddDays(subscription.SingleResult.FreeTrialDuration).AddYears(selectedDuration) : DateTime.UtcNow.AddYears(selectedDuration),
                    DateCreated = DateTime.UtcNow                    
                };

                var resp = _subManager.RegisterOrganizationSubscription(org, usr, pay, sub).Result;

                if (resp.StatusCode != (int)HttpStatusCode.OK)
                {
                    _logger.LogInformation($"Unable to Register Organization {org.Name}");

                    return View("Checkout", new CheckoutVM
                    {
                        StatusCode = (int)HttpStatusCode.ExpectationFailed,
                        Message = $"Unable to Register Organisation  {org.Name}",
                        SubscriptionPlan = subscription.SingleResult,
                        PaymentProvider = _payManager.FetchPaymentOptions().Result.Result.ToList(),
                        Country = _opsManager.FetchOperationalCountry().Result.Result.ToList()
                    });
                }

                HttpContext.Session.SetString("OrganisationSubscriptionStatus", "true");
                //get user's detail
                var user = _usrManager.GetUserByEmail(User.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value).Result;
                SetSessionVariables(user.SingleResult);

                //send OTP email
                var re = _mail.sendEmail(org.AdminEmailAddress, "Welcome to CITracker", "CITracker", _mail.PopulateRegistrationBody(org.Name));

                return RedirectToAction("Dashboard", "Main");
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
                _logger.LogError($"Exception at MakePayment || - {JsonConvert.SerializeObject(ex)}");

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
                if ((bool)user?.IsOrganizationSubscribed)
                {
                    HttpContext.Session.SetString("OrganisationSubscriptionStatus", "true");
                }
                else
                {
                    HttpContext.Session.SetString("OrganisationSubscriptionStatus", "false");
                }
            }
            else
            {
                HttpContext.Session.SetString("OrganisationSubscriptionStatus", "false");
            }
        } 


        private decimal CalculateSubscriptionAmount(SubscriptionPlan plan, int selectedDuration)
        {
            decimal amount = 0;
            if(plan != null)
            {
                amount = plan.Amount * selectedDuration;
            }
            return amount;
        }
    }
}
