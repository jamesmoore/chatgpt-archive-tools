using System.IO;
using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace ChatGPTExport
{
    public partial class ConversationFinder
    {
        private const string SearchPattern = "conversations*.json";
        private static readonly Regex ConversationFilePattern = ConversationFileRegex();

        public IEnumerable<IFileInfo> GetConversationFiles(IEnumerable<IDirectoryInfo> sources)
        {
            var conversationFiles = sources.Select(GetConversationFiles).
                SelectMany(fileInfo => fileInfo).ToList();
            return conversationFiles;
        }

        public IEnumerable<IFileInfo> GetConversationFiles(IDirectoryInfo sourceDir)
        {
            return sourceDir.Exists
                ? sourceDir.GetFiles(SearchPattern, SearchOption.AllDirectories)
                    .Where(f => ConversationFilePattern.IsMatch(f.Name))
                : [];
        }

        [GeneratedRegex(@"^conversations(-\d{3})?\.json$")]
        private static partial Regex ConversationFileRegex();
    }
}
