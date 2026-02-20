using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGPTExport.Visitor
{
    public static class VisitorExtensions
    {
        public static T? Accept<T>(this Message message, IContentVisitor<T> visitor)
        {
            if (message.content != null &&
                message.author?.role != null &&
                message.metadata != null &&
                message.recipient != null)
            {
                return message.content.Accept(visitor, new MessageContext(message.author, message.GetCreateTime(), message.GetUpdateTime(), message.metadata, message.recipient));
            }
            else
            {
                return default;
            }
        }

        public static T Accept<T>(this ContentBase content, IContentVisitor<T> visitor, MessageContext context)
        {
            return visitor.Visit(content, context);
        }
    }
}
