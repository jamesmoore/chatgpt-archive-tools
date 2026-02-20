using ChatGPTExport.Assets;
using ChatGPTExport.Decoders;
using ChatGPTExport.Formatters;
using ChatGPTExport.Formatters.Markdown;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Formatters.Markdown;

public class MarkdownContentVisitorTests
{
    [Fact]
    public void Visitor_DelegatesToCorrectDecoder_ForContentText()
    {
        var factory = new MarkdownDecoderFactory(
            new NullAssetRenderer(),
            new ConversationContext(),
            showHidden: false);
        var visitor = new MarkdownContentVisitor(factory);
        
        var content = new ContentText { parts = ["test"] };
        var context = new MessageContext(
            new Author() { role = "user" },
            null,
            null,
            new MessageMetadata(),
            string.Empty);

        var result = visitor.Visit(content, context);

        Assert.NotNull(result);
    }

    private class NullAssetRenderer : IMarkdownAssetRenderer
    {
        public IEnumerable<string> RenderAsset(MessageContext context, string asset_pointer) => [];
    }
}
