using System.IO.Abstractions;

namespace ChatGPTExport
{
    public class ConversationExportDirectory
    {
        public required IDirectoryInfo DirectoryInfo { get; set; }
        public required IEnumerable<IFileInfo> ConversationFiles { get; set; }

        // add concept of exists

    }
}
