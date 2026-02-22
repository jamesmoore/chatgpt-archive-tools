using System.IO;

namespace ChatGPTExport.Formatters
{
    public interface IFormattedConversationAsset
    {
        public string Name { get; }

        public Stream GetStream();

        public string MimeType { get; }
    }
}
