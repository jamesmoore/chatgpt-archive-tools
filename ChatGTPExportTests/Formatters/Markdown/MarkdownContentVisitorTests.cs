using ChatGPTExport.Assets;
using ChatGPTExport.Decoders;
using ChatGPTExport.Decoders.AssetRenderer;
using ChatGPTExport.Models;
using ChatGPTExport.Visitor;

namespace ChatGTPExportTests.Formatters.Markdown;

public class MarkdownContentVisitorTests
{
    [Fact]
    public void Visitor_DelegatesToCorrectDecoder_ForContentText()
    {
        ConversationContext conversationContext = new();
        var visitor = new MarkdownContentVisitor(
            new AssetLocator(),
            new NullAssetRenderer());
        
        var content = new ContentText { parts = ["test"] };
        var context = new MessageContext(
            new Author() { role = "user" },
            null,
            null,
            new MessageMetadata(),
            string.Empty,
            conversationContext,
            ShowHidden: false
            );

        var result = visitor.Visit(content, context);

        Assert.NotNull(result);
    }

    private class NullAssetRenderer : IMarkdownAssetRenderer
    {
        public string RenderAsset(Asset? asset, string asset_pointer) => string.Empty;
    }

    private class AssetLocator : IAssetLocator
    {
        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest) => null;
    }
}
