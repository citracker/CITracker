using Microsoft.AspNetCore.Http;

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
    }
}
