using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class ContentTetherBrowsingDisplayDecoderTests
{
    private static ContentTetherBrowsingDisplayDecoder CreateDecoder() => new();

    private static MessageContext CreateContext(bool showHidden)
    {
        return new MessageContext(new Author { role = "assistant" }, null, null, new MessageMetadata(), "all", new ConversationContext(), showHidden);
    }

    [Fact]
    public void TetherBrowsingDisplay_IsFilteredWhenShowHiddenIsFalse()
    {
        var decoder = CreateDecoder();
        var content = new ContentTetherBrowsingDisplay { result = "Result", summary = "Summary" };

        var result = decoder.Decode(content, CreateContext(showHidden: false));

        Assert.Empty(result.Lines);
    }

    [Fact]
    public void TetherBrowsingDisplay_IsRenderedWhenShowHiddenIsTrue()
    {
        var decoder = CreateDecoder();
        var content = new ContentTetherBrowsingDisplay { result = "Line1\nLine2", summary = "Summary" };

        var result = decoder.Decode(content, CreateContext(showHidden: true));

        Assert.Equal(["Line1  \nLine2", "Summary"], result.Lines.ToArray());
    }
}
