using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class ContentUserEditableContextDecoderTests
{
    private static ContentUserEditableContextDecoder CreateDecoder() => new();

    private static MessageContext CreateContext(bool showHidden)
    {
        return new MessageContext(new Author { role = "assistant" }, null, null, new MessageMetadata(), "all", new ConversationContext(), showHidden);
    }

    [Fact]
    public void UserEditableContext_IsFilteredWhenShowHiddenIsFalse()
    {
        var decoder = CreateDecoder();
        var content = new ContentUserEditableContext { user_profile = "profile", user_instructions = "instructions" };

        var result = decoder.Decode(content, CreateContext(showHidden: false));

        Assert.Empty(result.Lines);
    }

    [Fact]
    public void UserEditableContext_IsRenderedWhenShowHiddenIsTrue()
    {
        var decoder = CreateDecoder();
        var content = new ContentUserEditableContext { user_profile = "profile", user_instructions = "instructions" };

        var result = decoder.Decode(content, CreateContext(showHidden: true));

        Assert.Equal([
            "**User profile:** profile  ",
            "**User instructions:** instructions  "
        ], result.Lines.ToArray());
    }
}
