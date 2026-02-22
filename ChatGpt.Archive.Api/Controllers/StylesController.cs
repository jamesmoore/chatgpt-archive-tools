using ChatGpt.Archive.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChatGpt.Archive.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StylesController(AssetsCache assetsCache, ILogger<StylesController> logger) : ControllerBase
    {
        private readonly ILogger<StylesController> _logger = logger;

        [HttpGet("{**path}")]
        public IActionResult Index(string path)
        {
            var safePath = path?.Replace("\r", string.Empty).Replace("\n", string.Empty);
            _logger.LogInformation("StylesController received request for path: {Path}", safePath);
            var foundAsset = assetsCache.Get($"/styles/{path}");
            return foundAsset == null ? NotFound() : File(foundAsset.GetStream(), foundAsset.MimeType);
        }
    }
}
