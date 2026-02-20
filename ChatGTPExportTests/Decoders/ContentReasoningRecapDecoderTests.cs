using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class ContentReasoningRecapDecoderTests
{
    private static ContentReasoningRecapDecoder CreateDecoder(bool showHidden = false) => new(showHidden);

    private static MessageContext CreateContext()
    {
        return new MessageContext(new Author { role = "assistant" }, null, null, new MessageMetadata(), "all");
    }

    [Fact]
    public void ReasoningRecap_IsFilteredWhenShowHiddenIsFalse()
    {
        var decoder = CreateDecoder(showHidden: false);
        var content = new ContentReasoningRecap { content = "summary" };

        var result = decoder.Decode(content, CreateContext());

        Assert.Empty(result.Lines);
    }

    [Fact]
    public void ReasoningRecap_IsReturnedWhenShowHiddenIsTrue()
    {
        var decoder = CreateDecoder(showHidden: true);
        var content = new ContentReasoningRecap { content = "summary" };

        var result = decoder.Decode(content, CreateContext());

        var line = Assert.Single(result.Lines);
        Assert.Equal("summary", line);
    }
}
