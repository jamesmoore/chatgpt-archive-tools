using ChatGPTExport.Assets;
using ChatGPTExport.Decoders;
using ChatGPTExport.Decoders.AssetRenderer;
using ChatGPTExport.Models;

namespace ChatGPTExport.Visitor
{
    public class MarkdownContentVisitor(
        IAssetLocator assetLocator,
        IMarkdownAssetRenderer assetRenderer) : IContentVisitor<MarkdownContentResult>
    {
        private readonly Lazy<ContentTextDecoder> _contentTextDecoder = new(() => new ContentTextDecoder());
        private readonly Lazy<ContentMultimodalTextDecoder> _contentMultimodalTextDecoder = new(() => new ContentMultimodalTextDecoder(assetLocator, assetRenderer));
        private readonly Lazy<ContentCodeDecoder> _contentCodeDecoder = new(() => new ContentCodeDecoder());
        private readonly Lazy<ContentThoughtsDecoder> _contentThoughtsDecoder = new(() => new ContentThoughtsDecoder());
        private readonly Lazy<ContentExecutionOutputDecoder> _contentExecutionOutputDecoder = new(() => new ContentExecutionOutputDecoder());
        private readonly Lazy<ContentReasoningRecapDecoder> _contentReasoningRecapDecoder = new(() => new ContentReasoningRecapDecoder());
        private readonly Lazy<ContentUserEditableContextDecoder> _contentUserEditableContextDecoder = new(() => new ContentUserEditableContextDecoder());
        private readonly Lazy<ContentTetherBrowsingDisplayDecoder> _contentTetherBrowsingDisplayDecoder = new(() => new ContentTetherBrowsingDisplayDecoder());
        private readonly Lazy<ContentComputerOutputDecoder> _contentComputerOutputDecoder = new(() => new ContentComputerOutputDecoder());
        private readonly Lazy<ContentSystemErrorDecoder> _contentSystemErrorDecoder = new(() => new ContentSystemErrorDecoder());
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