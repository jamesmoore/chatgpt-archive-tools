using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGPTExport.Formatters
{
    public interface IConversationFormatter
    {
        FormattedConversation Format(IMarkdownAssetRenderer assetLocator, Conversation conversation, string pathPrefix);
    }
}