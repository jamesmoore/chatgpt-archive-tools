using ChatGPTExport.Formatters.Html;
using ChatGPTExport.Formatters.Html.Template;

namespace ChatGTPExportTests.Formatters.Html;

public class HtmlFormatterTests
{
    [Fact]
    public void TailwindHtmlFormatter_FormatHtmlPage_ProducesValidOutput()
    {
        // Arrange
        var page = new HtmlPage(
            "Test Title",
            new[] { "<meta name='test' content='value'>" },
            new[]
            {
                new HtmlFragment(false, "<p>Assistant message</p>", false, false, false, Array.Empty<string>()),
                new HtmlFragment(true, "<p>User message</p>", false, false, false, Array.Empty<string>())
            }
        );

        var formatter = new TailwindHtmlFormatter();

        // Act
        var html = formatter.FormatHtmlPage(page);

        // Assert
        Assert.NotEmpty(html);
        Assert.Contains("Test Title", html);
        Assert.Contains("Assistant message", html);
        Assert.Contains("User message", html);
        Assert.Contains("meta name='test'", html);
        Assert.Contains("styles/compiled.css", html);
        Assert.Contains("<!doctype html>", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TailwindHtmlFormatter_FormatHtmlPage_DifferentiatesUserMessages()
    {
        // Arrange
        var page = new HtmlPage(
            "Test",
            Array.Empty<string>(),
            new[]
            {
                new HtmlFragment(false, "<p>Assistant</p>", false, false, false, Array.Empty<string>()),
                new HtmlFragment(true, "<p>User</p>", false, false, false, Array.Empty<string>())
            }
        );

        var formatter = new TailwindHtmlFormatter();

        // Act
        var html = formatter.FormatHtmlPage(page);

        // Assert - Tailwind uses specific classes for user messages
        Assert.Contains("justify-end", html);
        Assert.Contains("rounded-xl", html);
    }
}
