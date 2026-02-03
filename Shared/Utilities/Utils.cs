using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace Shared.Utilities
{
    public class Utils
    {
        public static bool IsValidWordDocument(IFormFile file)
        {
            var allowedMimeTypes = new[]
            {
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/msword"
            };

            if (!allowedMimeTypes.Contains(file.ContentType))
                return false;

            using var stream = file.OpenReadStream();

            try
            {
                if (file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                {
                    using var doc = DocumentFormat.OpenXml.Packaging
                        .WordprocessingDocument.Open(stream, false);

                    return doc.MainDocumentPart != null;
                }

                // .doc (binary Word)
                return stream.Length > 512; // basic sanity check
            }
            catch
            {
                return false;
            }
        }

        public static bool BeValidIsoDate(string value)
        {
            return DateTime.TryParseExact(
                value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);
        }

        public static readonly HashSet<string> SavingsClassificationAllowedValues =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Cost Out",
            "Cost Savings",
            "Revenue",
            "Cost Avoidance"
        };

        public static readonly HashSet<string> PriorityAllowedValues =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "High",
            "Medium",
            "Low"
        };

        public static readonly HashSet<string> CertificationAllowedValues =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Green Belt",
            "Black Belt",
            "PMP",
            "Not Applicable"
        };

        public static readonly HashSet<string> CarryOverAllowedValues =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Yes",
            "No"
        };


        private static readonly Dictionary<string, string> CurrencySymbols =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Major
            { "USD", "$" },
            { "EUR", "€" },
            { "GBP", "£" },
            { "JPY", "¥" },
            { "CNY", "¥" },
            { "CHF", "CHF" },
            { "CAD", "$" },
            { "AUD", "$" },
            { "NZD", "$" },

            // Americas
            { "MXN", "$" },
            { "BRL", "R$" },
            { "ARS", "$" },
            { "CLP", "$" },
            { "COP", "$" },
            { "PEN", "S/" },

            // Europe
            { "SEK", "kr" },
            { "NOK", "kr" },
            { "DKK", "kr" },
            { "PLN", "zł" },
            { "CZK", "Kč" },
            { "HUF", "Ft" },
            { "RON", "lei" },
            { "BGN", "лв" },

            // Middle East
            { "AED", "د.إ" },
            { "SAR", "﷼" },
            { "ILS", "₪" },
            { "QAR", "﷼" },
            { "KWD", "د.ك" },

            // Africa
            { "ZAR", "R" },
            { "NGN", "₦" },
            { "EGP", "£" },
            { "KES", "KSh" },
            { "GHS", "₵" },

            // Asia
            { "INR", "₹" },
            { "PKR", "₨" },
            { "LKR", "Rs" },
            { "BDT", "৳" },
            { "SGD", "$" },
            { "HKD", "$" },
            { "KRW", "₩" },
            { "THB", "฿" },
            { "MYR", "RM" },
            { "IDR", "Rp" },
            { "PHP", "₱" },
            { "VND", "₫" },

            // Others
            { "RUB", "₽" },
            { "UAH", "₴" },
            { "TRY", "₺" },
            { "IRR", "﷼" },
            { "ISK", "kr" }
        };

        /// <summary>
        /// Returns the currency symbol for a given ISO 4217 code.
        /// Returns null if the currency is unknown.
        /// </summary>
        public static string? GetSymbol(string iso4217Code)
        {
            if (string.IsNullOrWhiteSpace(iso4217Code))
                return null;

            return CurrencySymbols.TryGetValue(iso4217Code.Trim(), out var symbol)
                ? symbol
                : null;
        }
    }
}
