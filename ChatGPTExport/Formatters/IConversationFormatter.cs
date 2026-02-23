using ChatGPTExport.Models;

namespace ChatGPTExport.Formatters
{
    public interface IConversationFormatter
    {
        FormattedConversation Format(Conversation conversation, string pathPrefix, bool showHidden);
    }
}