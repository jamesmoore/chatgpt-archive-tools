using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGPTExport.Formatters
{
    public interface IConversationFormatter
    {
        IEnumerable<string> Format(IMarkdownAssetRenderer assetLocator, Conversation conversation);
        string GetExtension();
    }
}