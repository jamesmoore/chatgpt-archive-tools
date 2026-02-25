using System.IO;
using System.IO.Abstractions;

namespace ChatGPTExport
{
    public class ConversationFinder
    {
        public IEnumerable<IFileInfo> GetConversationFiles(IEnumerable<IDirectoryInfo> sources)
        {
            var conversationFiles = sources.Select(GetConversationFiles).
                SelectMany(fileInfo => fileInfo).ToList();
            return conversationFiles;
        }

        public IEnumerable<IFileInfo> GetConversationFiles(IDirectoryInfo sourceDir)
        {
            return sourceDir.Exists
                ? sourceDir.GetFiles("conversations*.json", SearchOption.AllDirectories)
                    .Where(ConversationsFileNameValidator.IsConversationFile)
                : [];
        }
    }
}
