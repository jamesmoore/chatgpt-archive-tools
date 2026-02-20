using ChatGPTExport.Assets;
using ChatGPTExport.Decoders;
using ChatGPTExport.Exporters;
using ChatGPTExport.Formatters;
using ChatGPTExport.Models;

namespace ChatGTPExportTests.Formatters.Markdown;

public class MarkdownContentVisitorTests
{
    private static MarkdownContentVisitor CreateVisitor()
    {
        NullAssetLocator nullAssetLocator = new();
        MarkdownAssetRenderer markdownAssetRenderer = new(nullAssetLocator);
        var showHidden = false;
        return new MarkdownContentVisitor(
            new ContentTextDecoder(new ConversationContext(), showHidden),
            new ContentMultimodalTextDecoder(markdownAssetRenderer),
            showHidden);
    }

    private static MessageContext CreateContext(string role)
    {
        return new MessageContext(new Author() { role = role }, null, null, new MessageMetadata(), string.Empty);
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

    [Fact]
    public void ZeroIndexFootnotes_AreHandledCorrectly()
    {
        var visitor = CreateVisitor();
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

        var context = new MessageContext(new Author() { role = "assistant" }, null, null, metadata, string.Empty);

        var result = visitor.Visit(content, context);

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
        var visitor = CreateVisitor();
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

        var context = new MessageContext(new Author() { role = "assistant" }, null, null, metadata, string.Empty);

        var result = visitor.Visit(content, context);

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
        var visitor = CreateVisitor();
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

        var context = new MessageContext(new Author() { role = "assistant" }, null, null, metadata, string.Empty);

        var result = visitor.Visit(content, context);

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

    private class NullAssetLocator : IAssetLocator
    {
        public Asset? GetMarkdownMediaAsset(AssetRequest assetRequest) => null;
    }
}
