using System.IO;
using System.IO.Abstractions;

namespace ChatGPTExport.Formatters
{
    public interface IFormattedConversationAsset
    {
        public string Name { get; }

        public Stream GetStream();

        public string MimeType { get; }

        public SaveStatus SaveToFileSystem(IDirectoryInfo directoryInfo);
    }

    public enum SaveStatus
    {
        Success,
        Exists,
        Failed,
    }
}
