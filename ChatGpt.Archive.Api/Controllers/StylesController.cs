using ChatGpt.Archive.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatGpt.Archive.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StylesController(AssetsCache assetsCache) : ControllerBase
    {
        [HttpGet("{**path}")]
        public IActionResult Index(string path)
        {
            Console.WriteLine("StyleController received request for path: " + path);
            var foundAsset = assetsCache.Get($"/styles/{path}");
            return foundAsset == null ? NotFound() : File(foundAsset.GetStream(), foundAsset.MimeType);
        }
    }
}
