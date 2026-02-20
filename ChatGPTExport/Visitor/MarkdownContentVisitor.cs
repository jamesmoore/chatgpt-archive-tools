using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGPTExport.Visitor
{
    public class MarkdownContentVisitor(MarkdownDecoderFactory decoders) : IContentVisitor<MarkdownContentResult>
    {
        public MarkdownContentResult Visit(ContentBase content, MessageContext context)
            => decoders.UnhandledContent.DecodeToMarkdown(content, context);

        public MarkdownContentResult Visit(ContentText content, MessageContext context)
            => decoders.ContentText.DecodeToMarkdown(content, context);

        public MarkdownContentResult Visit(ContentMultimodalText content, MessageContext context)
            => decoders.ContentMultimodalText.DecodeToMarkdown(content, context);

        public MarkdownContentResult Visit(ContentCode content, MessageContext context)
            => decoders.ContentCode.DecodeToMarkdown(content, context);

        public MarkdownContentResult Visit(ContentThoughts content, MessageContext context)
            => decoders.ContentThoughts.DecodeToMarkdown(content, context);

        public MarkdownContentResult Visit(ContentExecutionOutput content, MessageContext context)
            => decoders.ContentExecutionOutput.DecodeToMarkdown(content, context);

        public MarkdownContentResult Visit(ContentReasoningRecap content, MessageContext context)
            => decoders.ContentReasoningRecap.DecodeToMarkdown(content, context);

        public MarkdownContentResult Visit(ContentUserEditableContext content, MessageContext context)
            => decoders.ContentUserEditableContext.DecodeToMarkdown(content, context);

        public MarkdownContentResult Visit(ContentTetherBrowsingDisplay content, MessageContext context)
            => decoders.ContentTetherBrowsingDisplay.DecodeToMarkdown(content, context);

        public MarkdownContentResult Visit(ContentComputerOutput content, MessageContext context)
            => decoders.ContentComputerOutput.DecodeToMarkdown(content, context);

        public MarkdownContentResult Visit(ContentSystemError content, MessageContext context)
            => decoders.ContentSystemError.DecodeToMarkdown(content, context);
    }
}
