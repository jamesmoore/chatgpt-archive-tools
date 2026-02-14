using ChatGpt.Archive.Api.Database;
using ChatGpt.Archive.Api.Services;
using ChatGPTExport;
using ChatGPTExport.Formatters.Html;
using ChatGPTExport.Formatters.Markdown;
using Microsoft.AspNetCore.Mvc;

namespace ChatGpt.Archive.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ManageController(
        IConversationsService conversationsService
        ) : ControllerBase
    {
        [HttpGet("clear")]
        public ActionResult<bool> Clear()
        {
            conversationsService.ClearAll();
            return Ok(true);
        }
    }
}

