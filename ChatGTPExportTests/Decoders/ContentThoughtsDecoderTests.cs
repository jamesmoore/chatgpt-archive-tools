using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class ContentThoughtsDecoderTests
{
    private static ContentThoughtsDecoder CreateDecoder() => new();

    private static MessageContext CreateContext(bool showHidden)
    {
        return new MessageContext(new Author { role = "assistant" }, null, null, new MessageMetadata(), "all", new ConversationContext(), showHidden);
    }

    [Fact]
    public void Thoughts_AreFilteredWhenShowHiddenIsFalse()
    {
        var decoder = CreateDecoder();
        var content = new ContentThoughts
        {
            thoughts = [new ContentThoughts.Thoughts { summary = "Summary", content = "Detail" }]
        };

        var result = decoder.Decode(content, CreateContext(showHidden: false));

        Assert.Empty(result.Lines);
    }

    [Fact]
    public void Thoughts_AreRenderedWithSuffixWhenShowHiddenIsTrue()
    {
        var decoder = CreateDecoder();
        var content = new ContentThoughts
        {
            thoughts = [new ContentThoughts.Thoughts { summary = "Summary", content = "Detail" }]
        };

        var result = decoder.Decode(content, CreateContext(showHidden: true));

        Assert.Equal(["Summary  ", "Detail  "], result.Lines.ToArray());
        Assert.Equal(" 💭", result.Suffix);
    }
}
