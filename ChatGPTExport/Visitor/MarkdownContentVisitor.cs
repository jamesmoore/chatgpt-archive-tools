using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGPTExport.Visitor
{
    public class MarkdownContentVisitor(
        IMarkdownAssetRenderer assetRenderer,
        ConversationContext conversationContext,
        bool showHidden) : IContentVisitor<MarkdownContentResult>
    {
        private readonly Lazy<ContentTextDecoder> _contentTextDecoder = new(() => new ContentTextDecoder(conversationContext, showHidden));
        private readonly Lazy<ContentMultimodalTextDecoder> _contentMultimodalTextDecoder = new(() => new ContentMultimodalTextDecoder(assetRenderer));
        private readonly Lazy<ContentCodeDecoder> _contentCodeDecoder = new(() => new ContentCodeDecoder(showHidden));
        private readonly Lazy<ContentThoughtsDecoder> _contentThoughtsDecoder = new(() => new ContentThoughtsDecoder(showHidden));
        private readonly Lazy<ContentExecutionOutputDecoder> _contentExecutionOutputDecoder = new(() => new ContentExecutionOutputDecoder(showHidden));
        private readonly Lazy<ContentReasoningRecapDecoder> _contentReasoningRecapDecoder = new(() => new ContentReasoningRecapDecoder(showHidden));
        private readonly Lazy<ContentUserEditableContextDecoder> _contentUserEditableContextDecoder = new(() => new ContentUserEditableContextDecoder(showHidden));
        private readonly Lazy<ContentTetherBrowsingDisplayDecoder> _contentTetherBrowsingDisplayDecoder = new(() => new ContentTetherBrowsingDisplayDecoder(showHidden));
        private readonly Lazy<ContentComputerOutputDecoder> _contentComputerOutputDecoder = new(() => new ContentComputerOutputDecoder());
        private readonly Lazy<ContentSystemErrorDecoder> _contentSystemErrorDecoder = new(() => new ContentSystemErrorDecoder(showHidden));
        private readonly Lazy<UnhandledContentDecoder> _unhandledContentDecoder = new(() => new UnhandledContentDecoder());

        public MarkdownContentResult Visit(ContentBase content, MessageContext context) => content switch
        {
            ContentText ct => _contentTextDecoder.Value.Decode(ct, context),
            ContentMultimodalText cmt => _contentMultimodalTextDecoder.Value.Decode(cmt, context),
            ContentCode cc => _contentCodeDecoder.Value.Decode(cc, context),
            ContentThoughts ct => _contentThoughtsDecoder.Value.Decode(ct, context),
            ContentExecutionOutput ceo => _contentExecutionOutputDecoder.Value.Decode(ceo, context),
            ContentReasoningRecap crr => _contentReasoningRecapDecoder.Value.Decode(crr, context),
            ContentUserEditableContext cuec => _contentUserEditableContextDecoder.Value.Decode(cuec, context),
            ContentTetherBrowsingDisplay ctbd => _contentTetherBrowsingDisplayDecoder.Value.Decode(ctbd, context),
            ContentComputerOutput cco => _contentComputerOutputDecoder.Value.Decode(cco, context),
            ContentSystemError cse => _contentSystemErrorDecoder.Value.Decode(cse, context),
            _ => _unhandledContentDecoder.Value.Decode(content, context)
        };
    }
}