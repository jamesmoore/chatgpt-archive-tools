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
            var sourceDirectories = archiveSourcesOptions.SourceDirectories.Select(fileSystem.DirectoryInfo.New);
            var conversations = sourceDirectories.Select(p => new SourceDirectory(p.FullName, p.Exists, GetConversationDirectoryArray(p)));
            return Ok(new Status(
                conversations.ToArray(),
                archiveSourcesOptions.DataDirectory,
                databaseConfiguration.DatabasePath
                ));
        }

        private ConversationDirectory[] GetConversationDirectoryArray(IDirectoryInfo p)
        {
            return conversationFinder.GetConversationFiles(p).Select(
                q => new ConversationDirectory(q.DirectoryInfo.FullName, q.ConversationFiles.Select(r => r.Name).ToArray())).ToArray();
        }

        public record Status(
            SourceDirectory[] SourceDirectories,
            string DataDirectory,
            string DatabasePath
            );

        public record SourceDirectory(string DirectoryName, bool Exists, ConversationDirectory[] ConversationDirectories);

        public record ConversationDirectory(string DirectoryName, string[] Conversations);
    }
}

