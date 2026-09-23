using System.Globalization;
using System.Security.Claims;

namespace CITracker.Helpers
{
    public static class ClaimsPrincipalNameExtensions
    {
        /// <summary>
        /// Resolves first and last name from a signed-in principal.
        /// Works for Microsoft Entra ID (OIDC + legacy WS-Fed claims) and Google (OIDC).
        /// Returns empty strings rather than null so callers can safely concatenate.
        /// </summary>
        public static (string FirstName, string LastName) ResolveName(this ClaimsPrincipal? principal)
        {
            if (principal?.Identity?.IsAuthenticated != true)
                return (string.Empty, string.Empty);

            // ── 1. OIDC standard claims (both providers emit these) ────────────
            var given = principal.FindFirst("given_name")?.Value
                      ?? principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value;

            var family = principal.FindFirst("family_name")?.Value
                      ?? principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")?.Value;

            if (!string.IsNullOrWhiteSpace(given) || !string.IsNullOrWhiteSpace(family))
                return (given ?? string.Empty, family ?? string.Empty);

            // ── 2. Fall back to "name" and split on the first space ────────────
            var fullName = principal.FindFirst("name")?.Value
                        ?? principal.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrWhiteSpace(fullName))
                return SplitFullName(fullName);

            // ── 3. Last resort: derive from the email local part ───────────────
            var email = principal.FindFirst("preferred_username")?.Value   // Microsoft
                     ?? principal.FindFirst("email")?.Value                // Google
                     ?? principal.FindFirst(ClaimTypes.Email)?.Value;

            if (!string.IsNullOrWhiteSpace(email) && email.Contains('@'))
            {
                var local = email.Split('@')[0]
                                 .Replace('.', ' ')
                                 .Replace('_', ' ')
                                 .Replace('-', ' ')
                                 .Trim();

                return SplitFullName(local, titleCase: true);
            }

            return (string.Empty, string.Empty);
        }

        private static (string, string) SplitFullName(string value, bool titleCase = false)
        {
            var parts = value.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

            string Casing(string s) => titleCase
                ? CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLowerInvariant())
                : s;

            return parts.Length switch
            {
                0 => (string.Empty, string.Empty),
                1 => (Casing(parts[0]), string.Empty),
                _ => (Casing(parts[0]), Casing(parts[1]))
            };
        }
    }
}
