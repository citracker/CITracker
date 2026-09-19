using CITracker.Helpers;
using CITracker.Validator;
using Datalayer.Implementations;
using Datalayer.Interfaces;
using DataRepository;
using FluentValidation;
using Infastructure.Implementation;
using Infastructure.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using NLog.Web;
using Shared;
using Shared.Implementations;
using Shared.Interfaces;
using Stripe;
using System;
using System.Globalization;

namespace CITracker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("es"), // Spanish
                new CultureInfo("pt"), // Portugese
                new CultureInfo("nl"), // dutch
                new CultureInfo("fr"), // french
                new CultureInfo("zh") // chinese
            };

            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add services to the container.
                builder.Services.AddDistributedMemoryCache();
                builder.Services.Configure<CookiePolicyOptions>(options =>
                {
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
                    options.Secure = CookieSecurePolicy.SameAsRequest;
                });

                builder.Services.AddControllers().AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });

                builder.Services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(Convert.ToInt32(builder.Configuration["AppSettings:SessionTimeout"]));
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.IsEssential = true;
                });

                builder.Services.AddAntiforgery(options =>
                {
                    //options.FormFieldName = "AntiforgeryFieldname";
                    options.HeaderName = "X-CSRF-TOKEN-HEADERNAME";
                    options.SuppressXFrameOptionsHeader = false;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.Path = "/";
                });

                builder.Services.AddRazorPages();

                builder.Services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);

                //builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                //    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
                //    .EnableTokenAcquisitionToCallDownstreamApi(new[] { "Organization.Read.All" })
                //    .AddInMemoryTokenCaches();

                var authBuilder = builder.Services
                    .AddAuthentication(options =>
                    {
                        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme; ;   // fallback provider
                    });

                //// 1. Cookies (required — every sign-in ends in a cookie)
                //authBuilder.AddCookie(options =>
                //{
                //    options.LoginPath = "/Home/SignIn";
                //    options.LogoutPath = "/Home/SignOut";
                //    options.AccessDeniedPath = "/Home/AccessDenied";
                //    options.ExpireTimeSpan = TimeSpan.FromMinutes(
                //        Convert.ToInt32(builder.Configuration["AppSettings:SessionTimeout"]));
                //    options.SlidingExpiration = true;
                //});

                // 2. Microsoft (multi-tenant) — note this DOES return the wrapper type
                authBuilder
                    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
                    .EnableTokenAcquisitionToCallDownstreamApi(new[] { "Organization.Read.All" })
                    .AddInMemoryTokenCaches();

                // 3. Google — chained on authBuilder (NOT on the Microsoft result)
                authBuilder.AddOpenIdConnect("Google", options =>
                {
                    options.Authority = "https://accounts.google.com";
                    options.ClientId = builder.Configuration["Google:ClientId"];
                    options.ClientSecret = builder.Configuration["Google:ClientSecret"];
                    options.CallbackPath = "/signin-google";
                    options.ResponseType = "code";
                    options.SaveTokens = true;
                    options.Scope.Add("openid");
                    options.Scope.Add("email");
                    options.Scope.Add("profile");
                });

                //// 4. Corporate SSO (any OIDC-compliant IdP)
                //authBuilder.AddOpenIdConnect("CorporateSso", options =>
                //{
                //    options.Authority = builder.Configuration["SAML:Authority"];
                //    options.ClientId = builder.Configuration["SAML:ClientId"];
                //    options.ClientSecret = builder.Configuration["SAML:ClientSecret"];
                //    options.CallbackPath = "/signin-corporate";
                //    options.ResponseType = "code";
                //    options.SaveTokens = true;
                //    options.Scope.Add("openid");
                //    options.Scope.Add("email");
                //    options.Scope.Add("profile");
                //});

                //// 5. Email/password (ASP.NET Core Identity)
                //builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
                //{
                //    options.SignIn.RequireConfirmedAccount = true;
                //    options.Password.RequiredLength = 10;
                //    options.User.RequireUniqueEmail = true;
                //})
                //.AddDefaultTokenProviders();

                //// Also needed by Identity if you want the UI endpoints:
                //builder.Services.AddRazorPages();


                builder.Services.AddAuthorization();

                builder.Services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                });

                builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/Home/SignIn";
                    options.LogoutPath = "/Home/SignOut";
                    options.AccessDeniedPath = "/Home/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(
                        Convert.ToInt32(builder.Configuration["AppSettings:SessionTimeout"]));
                    options.SlidingExpiration = true;
                });

                builder.Services.AddControllersWithViews()
                    .AddViewLocalization()
                    .AddDataAnnotationsLocalization();

                builder.Services.AddLocalization(options =>
                {
                    options.ResourcesPath = "Resources";
                });

                builder.Services.Configure<RequestLocalizationOptions>(options =>
                {
                    options.DefaultRequestCulture = new RequestCulture("en-US");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;

                    // Add our custom route provider at the beginning (highest priority)
                    options.RequestCultureProviders.Insert(0, new RouteCultureProvider(Options.Create(options)));

                    //options.RequestCultureProviders = new IRequestCultureProvider[]
                    //{
                    //    new CookieRequestCultureProvider(),
                    //    new AcceptLanguageHeaderRequestCultureProvider(),
                    //    new QueryStringRequestCultureProvider()
                    //};
                });

                builder.Services.Configure<KeyValues>(builder.Configuration.GetSection("AppSettings"));
                builder.Services.Configure<ADKeyValues>(builder.Configuration.GetSection("AzureAd"));
                builder.Services.Configure<StripeKeyValues>(builder.Configuration.GetSection("Stripe"));
                builder.Services.AddHttpClient();
                builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                builder.Services.AddSingleton<IMemoryCacheManager, MemoryCacheManager>();
                builder.Services.AddTransient<IPathProvider, PathProvider>();
                builder.Services.AddTransient<Mailer>();
                builder.Services.AddTransient<IGenericManager, GenericManager>();
                builder.Services.AddTransient<ISubscriptionManager, SubscriptionManager>();
                builder.Services.AddTransient<IPaymentManager, PaymentManager>();
                builder.Services.AddTransient<IOperationManager, OperationManager>();
                builder.Services.AddTransient<IUserManager, UserManager>();
                builder.Services.AddTransient<IAppSettingsManager, AppSettingsManager>();
                builder.Services.AddTransient<IRepository, Repository>();
                builder.Services.AddTransient<IMicrosoftOperations, MicrosoftOperations>();
                builder.Services.AddTransient<IStripePayment, StripePayment>();
                builder.Services.AddTransient<ISeatService, SeatService>();
                builder.Services.AddValidatorsFromAssemblyContaining<CIRequestValidator>();

                StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

                builder.Logging.ClearProviders();
                builder.Host.UseNLog();

                var app = builder.Build();

                // Configure the HTTP request pipeline
                app.UseForwardedHeaders();
                ////if (!app.Environment.IsDevelopment())
                ////{
                ////    app.UseExceptionHandler("/Error");
                    app.UseHsts();
                ////}
                ////else
                ////{
                    app.UseDeveloperExceptionPage();
                ////}

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();

                app.UseRequestLocalization();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseSession();
                app.UseStaticFiles();

                app.Use(async (context, next) =>
                {
                    string path = context.Request.Path;

                    if (path.EndsWith(".css") || path.EndsWith(".js"))
                    {
                        //Set css and js files to be cached for 7 days
                        TimeSpan maxAge = new TimeSpan(7, 0, 0, 0);     //7 days
                        context.Response.Headers.Append("Cache-Control", "max-age=" + maxAge.TotalSeconds.ToString());
                    }
                    else
                    {
                        //Request for views fall here.
                        context.Response.Headers.Append("Cache-Control", "no-cache");
                        context.Response.Headers.Append("Cache-Control", "private, no-store");
                    }

                    // Do work that doesn't write to the Response.
                    if (context.Request.Method == "OPTIONS")
                    {
                        context.Response.StatusCode = 405;
                        return;
                    }
                    //        context.Response.Headers.Add("Content-Security-Policy", "default-src 'self' ; script-src 'self' unpkg.com code.jquery.com cdnjs.cloudflare.com cdn.jsdelivr.net 'unsafe-inline'; style-src 'self' fonts.googleapis.com 'unsafe-inline'; img-src 'self'; connect-src 'self' wss://localhost:* 'unsafe-inline'; font-src 'self' fonts.googleapis.com fonts.gstatic.com data: 'unsafe-inline'; frame-src 'self' ;");
                    context.Response.Headers.Add("X-Frame-Options", "DENY");
                    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                    context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", "none");

                    await next();
                });

                app.UseEndpoints(endpoints =>
                {
                    // IMPORTANT: MapRazorPages first
                    endpoints.MapRazorPages();

                    // Culture route - handles /es, /pt, /fr, etc.
                    // This must come BEFORE the default route
                    endpoints.MapControllerRoute(
                        name: "culture",
                        pattern: "{culture}/{controller=Home}/{action=Index}/{id?}",
                        defaults: new { controller = "Home", action = "Index" });

                    // Default route - handles /, /home, /about, etc.
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");

                    // Optional: API routes if you have them
                    endpoints.MapControllers();
                });

                app.Run();
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
    }
}
