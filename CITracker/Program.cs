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
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using NLog.Extensions.Logging;
using Shared;
using Shared.Implementations;
using Shared.Interfaces;
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
                new CultureInfo("es")     // Spanish
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
                    options.JsonSerializerOptions.PropertyNamingPolicy = null; // Case-sensitive matching
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

                builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
                builder.Services.AddAuthorization();

                builder.Services.AddLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    logging.AddNLog();
                });

                builder.Services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                });

                builder.Services.AddLocalization(options =>
                {
                    options.ResourcesPath = "Resources";
                });

                builder.Services.AddControllersWithViews()
                    .AddViewLocalization()
                    .AddDataAnnotationsLocalization();

                builder.Services.Configure<RequestLocalizationOptions>(options =>
                {
                    options.DefaultRequestCulture = new RequestCulture("en-US");
                    options.SupportedCultures = supportedCultures;
                    options.SupportedUICultures = supportedCultures;

                    options.RequestCultureProviders = new IRequestCultureProvider[]
                    {
                        new CookieRequestCultureProvider(),
                        new AcceptLanguageHeaderRequestCultureProvider()
                    };
                });

                builder.Services.Configure<KeyValues>(builder.Configuration.GetSection("AppSettings"));
                builder.Services.Configure<ADKeyValues>(builder.Configuration.GetSection("AzureAd"));
                builder.Services.AddTransient<HttpClient>();
                builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                builder.Services.AddSingleton<IMemoryCacheManager, MemoryCacheManager>();
                builder.Services.AddTransient<IPathProvider, PathProvider>();
                builder.Services.AddTransient<Mailer>();
                //builder.Services.AddTransient<HelperFunctions>();
                builder.Services.AddTransient<IMicrosoftOperations, MicrosoftOperations>();
                builder.Services.AddTransient<IGenericManager, GenericManager>();
                builder.Services.AddTransient<ISubscriptionManager, SubscriptionManager>();
                builder.Services.AddTransient<IPaymentManager, PaymentManager>();
                builder.Services.AddTransient<IOperationManager, OperationManager>();
                builder.Services.AddTransient<IUserManager, UserManager>();
                builder.Services.AddTransient<IConnectionStringsManager, ConnectionStringsManager>();
                builder.Services.AddTransient<IRepository, Repository>();
                builder.Services.AddValidatorsFromAssemblyContaining<CIRequestValidator>();

                var app = builder.Build();

                // Configure the HTTP request pipeline
                app.UseForwardedHeaders();
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Error");
                    app.UseHsts();
                }
                else
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseRequestLocalization();
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseSession();
                app.UseStaticFiles();
                app.MapRazorPages();

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
                    endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
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
