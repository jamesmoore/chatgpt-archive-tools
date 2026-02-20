using ChatGPTExport.Formatters;

namespace ChatGPTExport.Decoders
{
    public class MarkdownDecoderFactory(
        IMarkdownAssetRenderer assetRenderer,
        ConversationContext conversationContext,
        bool showHidden)
    {
        public UnhandledContentDecoder UnhandledContent { get; } = new();
        public ContentTextDecoder ContentText { get; } = new(conversationContext, showHidden);
        public ContentMultimodalTextDecoder ContentMultimodalText { get; } = new(assetRenderer);
        public ContentCodeDecoder ContentCode { get; } = new(showHidden);
        public ContentThoughtsDecoder ContentThoughts { get; } = new(showHidden);
        public ContentExecutionOutputDecoder ContentExecutionOutput { get; } = new(showHidden);
        public ContentReasoningRecapDecoder ContentReasoningRecap { get; } = new(showHidden);
        public ContentUserEditableContextDecoder ContentUserEditableContext { get; } = new(showHidden);
        public ContentTetherBrowsingDisplayDecoder ContentTetherBrowsingDisplay { get; } = new(showHidden);
        public ContentComputerOutputDecoder ContentComputerOutput { get; } = new();
        public ContentSystemErrorDecoder ContentSystemError { get; } = new(showHidden);
    }
}