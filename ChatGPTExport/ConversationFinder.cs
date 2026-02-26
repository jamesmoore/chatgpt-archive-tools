using System.IO;
using System.IO.Abstractions;

namespace ChatGPTExport
{
    public class ConversationFinder
    {
        public IEnumerable<ConversationExportDirectory> GetConversationFiles(IEnumerable<IDirectoryInfo> sources)
        {
            var conversationFiles = sources.Select(GetConversationFiles).
                SelectMany(fileInfo => fileInfo).ToList();
            return conversationFiles;
        }

        public IEnumerable<ConversationExportDirectory> GetConversationFiles(IDirectoryInfo sourceDir)
        {
            var files = sourceDir.Exists
                ? sourceDir.GetFiles("conversations*.json", SearchOption.AllDirectories)
                    .Where(ConversationsFileNameValidator.IsConversationFile)
                    .OrderBy(p => p.FullName)
                : Enumerable.Empty<IFileInfo>();

            var grouped = files.Where(p => p.Directory != null).GroupBy(p => p.Directory!.FullName)
                .Select(g => new ConversationExportDirectory
                {
                    DirectoryInfo = g.First().Directory!,
                    ConversationFiles = g.ToList()
                }).ToList();

            return grouped;
        }
    }
}
