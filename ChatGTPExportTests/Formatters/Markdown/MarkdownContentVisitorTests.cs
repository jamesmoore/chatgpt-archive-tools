using ChatGPTExport.Assets;
using ChatGPTExport.Exporters;
using ChatGPTExport.Formatters;
using ChatGPTExport.Formatters.Markdown;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Formatters.Markdown;

public class MarkdownContentVisitorTests
{
    private static MarkdownContentVisitor CreateVisitor()
    {
        NullAssetLocator nullAssetLocator = new();
        MarkdownAssetRenderer markdownAssetRenderer = new(nullAssetLocator);
        return new MarkdownContentVisitor(markdownAssetRenderer, showHidden: false);
    }
      
    private static ContentVisitorContext CreateContext(string role)
    {
        return new ContentVisitorContext(new Author() { role = role }, null, null, new MessageMetadata(), string.Empty);
    }

    [Fact]
    public void UserMessages_AreSanitized()
    {
        var visitor = CreateVisitor();
        var content = new ContentText { parts = ["<script>alert('x')</script>"] };
        var context = CreateContext("user");

        var result = visitor.Visit(content, context);

        var line = Assert.Single(result.Lines);
        Assert.Equal("&lt;script&gt;alert('x')&lt;/script&gt;", line);
    }

    [Fact]
    public void NonUserMessages_AreNotSanitized()
    {
        var visitor = CreateVisitor();
        var content = new ContentText { parts = ["<script>alert('x')</script>"] };
        var context = CreateContext("assistant");

        var result = visitor.Visit(content, context);

        var line = Assert.Single(result.Lines);
        Assert.Equal("<script>alert('x')</script>", line);
    }

    private class NullAssetLocator : IAssetLocator
    {
        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest) => null;
    }
}
