using System.IO;
using System.IO.Abstractions;

namespace ChatGPTExport
{
    public class ConversationFinder
    {
        private const string SearchPattern = "conversations.json";

        public IEnumerable<IFileInfo> GetConversationFiles(IEnumerable<IDirectoryInfo> sources)
        {
            var conversationFiles = sources.Select(GetConversationFiles).
                SelectMany(fileInfo => fileInfo).ToList();
            return conversationFiles;
        }

        public IEnumerable<IFileInfo> GetConversationFiles(IDirectoryInfo sourceDir)
        {
            return sourceDir.Exists ? sourceDir.GetFiles(SearchPattern, SearchOption.AllDirectories) : [];
        }
    }
}
