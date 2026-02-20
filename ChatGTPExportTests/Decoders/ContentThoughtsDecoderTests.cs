using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class ContentThoughtsDecoderTests
{
    private static ContentThoughtsDecoder CreateDecoder(bool showHidden = false) => new(showHidden);

    private static MessageContext CreateContext()
    {
        return new MessageContext(new Author { role = "assistant" }, null, null, new MessageMetadata(), "all");
    }

    [Fact]
    public void Thoughts_AreFilteredWhenShowHiddenIsFalse()
    {
        var decoder = CreateDecoder(showHidden: false);
        var content = new ContentThoughts
        {
            thoughts = [new ContentThoughts.Thoughts { summary = "Summary", content = "Detail" }]
        };

        var result = decoder.DecodeTo(content, CreateContext());

        Assert.Empty(result.Lines);
    }

    [Fact]
    public void Thoughts_AreRenderedWithSuffixWhenShowHiddenIsTrue()
    {
        var decoder = CreateDecoder(showHidden: true);
        var content = new ContentThoughts
        {
            thoughts = [new ContentThoughts.Thoughts { summary = "Summary", content = "Detail" }]
        };

        var result = decoder.DecodeTo(content, CreateContext());

        Assert.Equal(["Summary  ", "Detail  "], result.Lines.ToArray());
        Assert.Equal(" 💭", result.Suffix);
    }
}
