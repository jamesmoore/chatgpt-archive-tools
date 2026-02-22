using System.Text.Json;
using System.Text.Json.Serialization;
using ChatGPTExport.Assets;
using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGPTExport.Formatters.Json
{
    internal class JsonFormatter : IConversationFormatter
    {
        private readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public FormattedConversation Format(IAssetLocator assetLocator, IMarkdownAssetRenderer assetRenderer, Conversation conversation, string pathPrefix)
        {
            return new FormattedConversation(JsonSerializer.Serialize(conversation, options), [], ".json");
        }
    }
}
