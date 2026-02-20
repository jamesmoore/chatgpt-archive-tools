using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class ContentUserEditableContextDecoderTests
{
    private static ContentUserEditableContextDecoder CreateDecoder(bool showHidden = false) => new(showHidden);

    private static MessageContext CreateContext()
    {
        return new MessageContext(new Author { role = "assistant" }, null, null, new MessageMetadata(), "all");
    }

    [Fact]
    public void UserEditableContext_IsFilteredWhenShowHiddenIsFalse()
    {
        var decoder = CreateDecoder(showHidden: false);
        var content = new ContentUserEditableContext { user_profile = "profile", user_instructions = "instructions" };

        var result = decoder.DecodeTo(content, CreateContext());

        Assert.Empty(result.Lines);
    }

    [Fact]
    public void UserEditableContext_IsRenderedWhenShowHiddenIsTrue()
    {
        var decoder = CreateDecoder(showHidden: true);
        var content = new ContentUserEditableContext { user_profile = "profile", user_instructions = "instructions" };

        var result = decoder.DecodeTo(content, CreateContext());

        Assert.Equal([
            "**User profile:** profile  ",
            "**User instructions:** instructions  "
        ], result.Lines.ToArray());
    }
}
