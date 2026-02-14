using ChatGpt.Archive.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatGpt.Archive.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ManageController(
        IConversationsService conversationsService
        ) : ControllerBase
    {
        [HttpDelete("cache")]  // DELETE /manage/cache
        public IActionResult ClearCache()
        {
            conversationsService.ClearAll();
            return Ok();
        }

        [HttpPost("cache")]    // POST /manage/cache (to rebuild/reload)
        public IActionResult RebuildCache()
        {
            conversationsService.LoadConversations();
            return Ok();
        }
    }
}

