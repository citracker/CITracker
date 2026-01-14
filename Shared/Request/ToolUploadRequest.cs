using Microsoft.AspNetCore.Http;

namespace Shared.Request
{
    public class ToolUploadRequest
    {
        public IFormFile File { get; set; }
        public long Id { get; set; }
        public string Tool { get; set; }
    }
}
