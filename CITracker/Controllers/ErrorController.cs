using CITracker.Helpers;
using Datalayer.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CITracker.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error")]
        public IActionResult Index()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            ViewBag.ErrorMessage = exceptionFeature?.Error?.Message;
            _logger.LogError(exceptionFeature?.Error, "Unhandled exception");
            return View();
        }
    }
}
