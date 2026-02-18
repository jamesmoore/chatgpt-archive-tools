using ChatGpt.Archive.Api.Database;
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
        DatabaseConfiguration databaseConfiguration,
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
                Select(fileSystem.DirectoryInfo.New).
                Select(p => 
                    p.Exists ? 
                    new SourceDirectory(p.FullName, true, conversationFinder.GetConversationFiles(p).Select(q => q.FullName).ToArray()) :
                    new SourceDirectory(p.FullName, false, [])
                    );
            return Ok(new Status(
                sourceDirectories.ToArray(), 
                archiveSourcesOptions.DataDirectory,
                databaseConfiguration.DatabasePath                
                ));
        }

        public record Status(
            SourceDirectory[] SourceDirectories, 
            string DataDirectory,
            string DatabasePath
            );

        public record SourceDirectory(string DirectoryName, bool Exists, string[] Conversations);
    }
}

