using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGPTExport.Visitor
{
    public static class VisitorExtensions
    {
        public static T? Accept<T>(this Message message, IContentVisitor<T> visitor, ConversationContext conversationContext, bool showHidden)
        {
            if (message.content != null &&
                message.author?.role != null &&
                message.metadata != null &&
                message.recipient != null)
            {
                MessageContext context = new(
                    message.author, 
                    message.GetCreateTime(), 
                    message.GetUpdateTime(), 
                    message.metadata, 
                    message.recipient, 
                    conversationContext,
                    showHidden);
                return message.content.Accept(visitor, context);
            }
            else
            {
                return default;
            }
        }

        private static T Accept<T>(this ContentBase content, IContentVisitor<T> visitor, MessageContext context)
        {
            return visitor.Visit(content, context);
        }
    }
}
