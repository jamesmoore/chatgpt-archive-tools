using System.IO;
using System.Reflection;

namespace ChatGPTExport.Formatters
{
    public interface IFormattedConversationAsset
    {
        public string Name { get; }

        public Stream GetStream();

        public string MimeType { get; }
    }

    public class EmbeddedResourceAsset(string name, string resourceName, string mimeType) : IFormattedConversationAsset
    {
        private static readonly Assembly Assembly = typeof(EmbeddedResourceAsset).Assembly;

        public string Name { get; } = name;

        public string MimeType { get; } = mimeType;

        public Stream GetStream()
        {
            return Assembly.GetManifestResourceStream(resourceName) ??
                throw new InvalidOperationException("Stream unavailable");
        }
    }
}
