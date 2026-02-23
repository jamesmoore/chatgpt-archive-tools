using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class ContentReasoningRecapDecoderTests
{
    private static ContentReasoningRecapDecoder CreateDecoder() => new();

    private static MessageContext CreateContext(bool showHidden)
    {
        return new MessageContext(new Author { role = "assistant" }, null, null, new MessageMetadata(), "all", new ConversationContext(), showHidden);
    }

    [Fact]
    public void ReasoningRecap_IsFilteredWhenShowHiddenIsFalse()
    {
        var decoder = CreateDecoder();
        var content = new ContentReasoningRecap { content = "summary" };

        var result = decoder.Decode(content, CreateContext(showHidden: false));

        Assert.Empty(result.Lines);
    }

    [Fact]
    public void ReasoningRecap_IsReturnedWhenShowHiddenIsTrue()
    {
        var decoder = CreateDecoder();
        var content = new ContentReasoningRecap { content = "summary" };

        var result = decoder.Decode(content, CreateContext(showHidden: true));

        var line = Assert.Single(result.Lines);
        Assert.Equal("summary", line);
    }
}
