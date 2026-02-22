using ChatGPTExport.Assets;
using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGPTExport.Formatters
{
    public interface IConversationFormatter
    {
        FormattedConversation Format(IAssetLocator assetLocator, IMarkdownAssetRenderer assetRenderer, Conversation conversation, string pathPrefix);
    }
}