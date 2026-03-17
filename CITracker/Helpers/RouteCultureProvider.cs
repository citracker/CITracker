using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace CITracker.Helpers
{
    public class RouteCultureProvider : IRequestCultureProvider
    {
        private readonly RequestLocalizationOptions _options;

        public RouteCultureProvider(IOptions<RequestLocalizationOptions> options)
        {
            _options = options.Value;
        }

        public Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var path = httpContext.Request.Path;

            Console.WriteLine($"RouteCultureProvider called for path: {httpContext.Request.Path}");

            // Get the first segment of the path
            var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // If no segments, let other providers handle it
            if (segments == null || segments.Length == 0)
            {
                return Task.FromResult<ProviderCultureResult>(null);
            }

            var potentialCulture = segments[0];

            if (potentialCulture != null)
            {
                Console.WriteLine($"Potential culture found: {potentialCulture}");
            }

            // Validate culture format (2 letters or 2-4 with dash, e.g., "pt", "en-US")
            if (!Regex.IsMatch(potentialCulture, @"^[a-z]{2}(?:-[a-zA-Z]{2,4})?$"))
            {
                return Task.FromResult<ProviderCultureResult>(null);
            }

            // Check if culture is supported
            var supported = _options.SupportedCultures?
                .Select(c => c.Name)
                .ToList() ?? new List<string>();

            // Add debug logging for supported cultures
            Console.WriteLine($"Supported cultures: {string.Join(", ", supported)}");
            Console.WriteLine($"Checking if '{potentialCulture}' is supported");

            // Check for exact match or two-letter prefix match
            var isSupported = supported.Contains(potentialCulture) ||
                              supported.Any(c => c.StartsWith(potentialCulture + "-")) ||
                              supported.Any(c => c.Split('-')[0] == potentialCulture);

            if (!isSupported)
            {
                Console.WriteLine($"Culture '{potentialCulture}' is not supported");
                return Task.FromResult<ProviderCultureResult>(null);
            }

            Console.WriteLine($"Culture '{potentialCulture}' is supported, applying it");

            // Return the culture - this will be used for both Culture and UICulture
            return Task.FromResult(new ProviderCultureResult(potentialCulture, potentialCulture));
        }
    }
}
