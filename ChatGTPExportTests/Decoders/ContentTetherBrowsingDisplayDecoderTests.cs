using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class ContentTetherBrowsingDisplayDecoderTests
{
    private static ContentTetherBrowsingDisplayDecoder CreateDecoder(bool showHidden = false) => new(showHidden);

    private static MessageContext CreateContext()
    {
        return new MessageContext(new Author { role = "assistant" }, null, null, new MessageMetadata(), "all");
    }

    [Fact]
    public void TetherBrowsingDisplay_IsFilteredWhenShowHiddenIsFalse()
    {
        var decoder = CreateDecoder(showHidden: false);
        var content = new ContentTetherBrowsingDisplay { result = "Result", summary = "Summary" };

        var result = decoder.DecodeToMarkdown(content, CreateContext());

        Assert.Empty(result.Lines);
    }

    [Fact]
    public void TetherBrowsingDisplay_IsRenderedWhenShowHiddenIsTrue()
    {
        var decoder = CreateDecoder(showHidden: true);
        var content = new ContentTetherBrowsingDisplay { result = "Line1\nLine2", summary = "Summary" };

        var result = decoder.DecodeToMarkdown(content, CreateContext());

        Assert.Equal(["Line1  \nLine2", "Summary"], result.Lines.ToArray());
    }
}
