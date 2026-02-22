using ChatGPTExport.Assets;
using ChatGPTExport.Decoders;
using ChatGPTExport.Models;
using ChatGPTExport.Visitor;

namespace ChatGTPExportTests.Formatters.Markdown;

public class MarkdownContentVisitorTests
{
    [Fact]
    public void Visitor_DelegatesToCorrectDecoder_ForContentText()
    {
        var visitor = new MarkdownContentVisitor(
            new AssetLocator(),
            new NullAssetRenderer(),
            new ConversationContext(),
            showHidden: false);
        
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
        public IEnumerable<string> RenderAsset(Asset? asset, string asset_pointer) => [];
    }

    private class AssetLocator : IAssetLocator
    {
        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest) => null;
    }
}
