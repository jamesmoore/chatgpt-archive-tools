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
            bool exists = sourceDir.Exists;
            var files = exists
                ? sourceDir.GetFiles("conversations*.json", SearchOption.AllDirectories)
                    .Where(ConversationsFileNameValidator.IsConversationFile)
                    .OrderBy(p => p.FullName)
                : Enumerable.Empty<IFileInfo>();

            var grouped = files.Where(p => p.Directory != null).GroupBy(p => p.Directory!.FullName)
                .Select(g => new ConversationExportDirectory
                {
                    DirectoryInfo = g.First().Directory!,
                    ConversationFiles = g.ToList(),
                    Exists = exists,
                }).ToList();

            return grouped;
        }
    }
}
