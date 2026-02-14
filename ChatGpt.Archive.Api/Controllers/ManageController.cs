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
        [HttpDelete]
        public ActionResult<bool> Clear()
        {
            conversationsService.ClearAll();
            return Ok(true);
        }

        [HttpPost("load")]
        public IActionResult LoadConversations()
        {
            conversationsService.LoadConversations();
            return Ok();
        }
    }
}

