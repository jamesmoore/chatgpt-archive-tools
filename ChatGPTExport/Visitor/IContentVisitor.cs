using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGPTExport.Visitor
{
    public interface IContentVisitor<T>
    {
        T Visit(ContentText content, MessageContext context);
        T Visit(ContentMultimodalText content, MessageContext context);
        T Visit(ContentCode content, MessageContext context);
        T Visit(ContentThoughts content, MessageContext context);
        T Visit(ContentExecutionOutput content, MessageContext context);
        T Visit(ContentReasoningRecap content, MessageContext context);
        T Visit(ContentBase content, MessageContext context);
        T Visit(ContentUserEditableContext content, MessageContext context);
        T Visit(ContentTetherBrowsingDisplay content, MessageContext context);
        T Visit(ContentComputerOutput content, MessageContext context);
        T Visit(ContentSystemError content, MessageContext context);
    }
}
