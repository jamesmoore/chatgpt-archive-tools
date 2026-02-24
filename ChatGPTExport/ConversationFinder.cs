using System.IO;
using System.IO.Abstractions;
using ChatGPTExport.Assets;

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
                    .Where(f => ConversationAssets.ConversationsFilePattern.IsMatch(f.Name))
                : [];
        }
    }
}
