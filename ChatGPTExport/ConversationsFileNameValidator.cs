using System.IO.Abstractions;
using System.Text.RegularExpressions;

namespace ChatGPTExport
{
    /// <summary>
    /// Validates whether a file has a valid conversations filename.
    /// </summary>
    public static partial class ConversationsFileNameValidator
    {
        private static readonly Regex ConversationsFilePattern = ConversationFileRegex();

        public static bool IsConversationFile(IFileInfo conversationsFile)
        {
            return ConversationsFilePattern.IsMatch(conversationsFile.Name);
        }

        [GeneratedRegex(@"^conversations(-\d{3})?\.json$")]
        private static partial Regex ConversationFileRegex();
    }
}
