using ChatGpt.Archive.Api.Services;
using ChatGPTExport;
using Microsoft.AspNetCore.Mvc;
using System.IO.Abstractions;

namespace ChatGpt.Archive.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ManageController(
        IConversationsService conversationsService,
        ArchiveSourcesOptions archiveSourcesOptions,
        ConversationFinder conversationFinder,
        IFileSystem fileSystem
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

        [HttpGet("status")]
        public ActionResult<Status> GetStatus()
        {
            var sourceDirectories = archiveSourcesOptions.SourceDirectories.
                Select(p => fileSystem.DirectoryInfo.New(p)).
                Select(p => new SourceDirectory()
                {
                    DirectoryName  = p.FullName,
                    Conversations = conversationFinder.GetConversationFiles(p).Select(p => p.FullName).ToArray(),
                });
            return Ok(new Status
            {
                SourceDirectories = sourceDirectories.ToArray(),
                DataDirectory = archiveSourcesOptions.DataDirectory,
            });
        }

        public class Status
        {
            public SourceDirectory[] SourceDirectories { get; set; }
            public string DataDirectory { get; set; }
        }

        public class SourceDirectory
        {
            public string DirectoryName { get; set; }
            public string[] Conversations { get; set; }
        }
    }
}

