using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGPTExport.Visitor
{
    public interface IContentVisitor<T>
    {
        T Visit(ContentBase content, MessageContext context);
    }
}
