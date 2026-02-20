using ChatGPTExport.Decoders;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Decoders;

public class ContentTextDecoderTests
{
    private static ContentTextDecoder CreateDecoder(bool showHidden = false)
    {
        return new ContentTextDecoder(new ConversationContext(), showHidden);
    }

    private static MessageContext CreateContext(string role, MessageMetadata? metadata = null)
    {
        metadata ??= new MessageMetadata();
        return new MessageContext(new Author() { role = role }, null, null, metadata, string.Empty);
    }

    [Fact]
    public void UserMessages_AreSanitized()
    {
        var decoder = CreateDecoder();
        var content = new ContentText { parts = ["<script>alert('x')</script>"] };
        var context = CreateContext("user");

        var result = decoder.Decode(content, context);

        var line = Assert.Single(result.Lines);
        Assert.Equal("&lt;script&gt;alert('x')&lt;/script&gt;", line);
    }

    [Fact]
    public void NonUserMessages_AreNotSanitized()
    {
        var decoder = CreateDecoder();
        var content = new ContentText { parts = ["<script>alert('x')</script>"] };
        var context = CreateContext("assistant");

        var result = decoder.Decode(content, context);

        var line = Assert.Single(result.Lines);
        Assert.Equal("<script>alert('x')</script>", line);
    }

    [Fact]
    public void ZeroIndexFootnotes_AreHandledCorrectly()
    {
        var decoder = CreateDecoder();
        var content = new ContentText { parts = ["Some text with references"] };

        var metadata = new MessageMetadata
        {
            content_references =
            [
                new MessageMetadata.Content_References
                {
                    type = "grouped_webpages",
                    start_idx = 0,
                    end_idx = 0,
                    items =
                    [
                        new MessageMetadata.Content_References.Item
                        {
                            title = "Zero Index Source",
                            url = "https://example.com/zero"
                        }
                    ]
                },
                new MessageMetadata.Content_References
                {
                    type = "grouped_webpages",
                    start_idx = 10,
                    end_idx = 20,
                    items =
                    [
                        new MessageMetadata.Content_References.Item
                        {
                            title = "Body Source",
                            url = "https://example.com/body"
                        }
                    ]
                }
            ]
        };

        var context = CreateContext("assistant", metadata);

        var result = decoder.Decode(content, context);

        var lines = result.Lines.ToArray();
        // Verify that zero-index references are inserted at the beginning
        Assert.Contains("[^1]", lines[0]);
        // Verify that footnotes are generated
        Assert.Contains("[^1]: [Zero Index Source](https://example.com/zero)", string.Join("\n", lines));
        Assert.Contains("[^2]: [Body Source](https://example.com/body)", string.Join("\n", lines));
    }

    [Fact]
    public void FallbackItemsWithGroupedWebpages_AreDeduplicated()
    {
        var decoder = CreateDecoder();
        var content = new ContentText { parts = ["Text with references"] };

        var metadata = new MessageMetadata
        {
            content_references =
            [
                new MessageMetadata.Content_References
                {
                    type = "grouped_webpages",
                    start_idx = 5,
                    end_idx = 10,
                    items =
                    [
                        new MessageMetadata.Content_References.Item
                        {
                            title = "Main Source",
                            url = "https://example.com/main"
                        },
                        new MessageMetadata.Content_References.Item
                        {
                            title = "Duplicate Source",
                            url = "https://example.com/duplicate"
                        }
                    ]
                },
                new MessageMetadata.Content_References
                {
                    type = "grouped_webpages_model_predicted_fallback",
                    start_idx = 12,
                    end_idx = 17,
                    items =
                    [
                        new MessageMetadata.Content_References.Item
                        {
                            title = "Duplicate Source",
                            url = "https://example.com/duplicate"
                        },
                        new MessageMetadata.Content_References.Item
                        {
                            title = "Fallback Only Source",
                            url = "https://example.com/fallback"
                        }
                    ]
                }
            ]
        };

        var context = CreateContext("assistant", metadata);

        var result = decoder.Decode(content, context);

        var output = string.Join("\n", result.Lines);

        // Verify that duplicate source appears only once
        var duplicateCount = System.Text.RegularExpressions.Regex.Matches(output, @"\[Duplicate Source\]").Count;
        Assert.Equal(1, duplicateCount);

        // Verify that fallback-only source is included
        Assert.Contains("[Fallback Only Source](https://example.com/fallback)", output);

        // Verify that main source is included
        Assert.Contains("[Main Source](https://example.com/main)", output);
    }

    [Fact]
    public void FootnoteNumbering_IsStableAndSequential()
    {
        var decoder = CreateDecoder();
        var content = new ContentText { parts = ["Reference one and reference two"] };

        var metadata = new MessageMetadata
        {
            content_references =
            [
                new MessageMetadata.Content_References
                {
                    type = "grouped_webpages",
                    start_idx = 10,
                    end_idx = 13,
                    items =
                    [
                        new MessageMetadata.Content_References.Item
                        {
                            title = "First Source",
                            url = "https://example.com/first"
                        }
                    ]
                },
                new MessageMetadata.Content_References
                {
                    type = "grouped_webpages",
                    start_idx = 28,
                    end_idx = 31,
                    items =
                    [
                        new MessageMetadata.Content_References.Item
                        {
                            title = "Second Source",
                            url = "https://example.com/second"
                        }
                    ]
                }
            ]
        };

        var context = CreateContext("assistant", metadata);

        var result = decoder.Decode(content, context);

        var lines = result.Lines.ToArray();
        var output = string.Join("\n", lines);

        // Verify footnote numbering is sequential starting from 1
        Assert.Contains("[^1]: [First Source](https://example.com/first)", output);
        Assert.Contains("[^2]: [Second Source](https://example.com/second)", output);

        // Verify the references in the text use the same numbers
        Assert.Contains("[^1]", lines[0]);
        Assert.Contains("[^2]", lines[0]);

        // Verify no gaps in numbering
        Assert.DoesNotContain("[^3]", output);
    }

    [Fact]
    public void PersonalizedContextMessages_AreHiddenWhenShowHiddenIsFalse()
    {
        var decoder = CreateDecoder(showHidden: false);
        var content = new ContentText { parts = ["Personalized context content"] };
        var context = new MessageContext(
            new Author() { role = "tool", name = "personalized_context" },
            null,
            null,
            new MessageMetadata(),
            string.Empty);

        var result = decoder.Decode(content, context);

        Assert.Empty(result.Lines);
    }

    [Fact]
    public void PersonalizedContextMessages_AreShownWhenShowHiddenIsTrue()
    {
        var decoder = CreateDecoder(showHidden: true);
        var content = new ContentText { parts = ["Personalized context content"] };
        var context = new MessageContext(
            new Author() { role = "tool", name = "personalized_context" },
            null,
            null,
            new MessageMetadata(),
            string.Empty);

        var result = decoder.Decode(content, context);

        Assert.NotEmpty(result.Lines);
        Assert.Contains("Personalized context content", string.Join("\n", result.Lines));
    }
}
