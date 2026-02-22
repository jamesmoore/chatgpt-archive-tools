using System.IO;
using System.Reflection;

namespace ChatGPTExport.Formatters
{
    // Include mime type?
    public interface IFormattedConversationAsset
    {
        public string Name { get; }

        public Stream GetStream();
    }

    public class EmbeddedResourceAsset : IFormattedConversationAsset
    {
        private static readonly Assembly Assembly = typeof(EmbeddedResourceAsset).Assembly;

        public EmbeddedResourceAsset(string name, string resourceName)
        {
            Name = name;
            this.resourceName = resourceName;
        }

        private string resourceName;

        public string Name { get; private set; }

        public Stream GetStream()
        {
            return Assembly.GetManifestResourceStream(resourceName) ??
                throw new InvalidOperationException("Stream unavailable"); ;
        }
    }
}
