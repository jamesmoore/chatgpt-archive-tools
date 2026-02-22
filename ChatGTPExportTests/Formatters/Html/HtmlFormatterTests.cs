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
            ["<meta name='test' content='value'>"],
            [
                new HtmlFragment(false, "<p>Assistant message</p>", false, false, false, []),
                new HtmlFragment(true, "<p>User message</p>", false, false, false, [])
            ],
            "styles/tailwindcompiled.css"
        );

        var formatter = new TailwindHtmlFormatter();

        // Act
        var html = formatter.FormatHtmlPage(page, string.Empty);

        // Assert
        Assert.NotEmpty(html);
        Assert.Contains("Test Title", html);
        Assert.Contains("Assistant message", html);
        Assert.Contains("User message", html);
        Assert.Contains("meta name='test'", html);
        Assert.Contains("styles/tailwindcompiled.css", html);
        Assert.Contains("<!doctype html>", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TailwindHtmlFormatter_FormatHtmlPage_DifferentiatesUserMessages()
    {
        // Arrange
        var page = new HtmlPage(
            "Test",
            [],
            [
                new HtmlFragment(false, "<p>Assistant</p>", false, false, false, []),
                new HtmlFragment(true, "<p>User</p>", false, false, false, [])
            ],
            "styles/tailwindcompiled.css"
        );

        var formatter = new TailwindHtmlFormatter();

        // Act
        var html = formatter.FormatHtmlPage(page, string.Empty);

        // Assert - Tailwind uses specific classes for user messages
        Assert.Contains("justify-end", html);
        Assert.Contains("rounded-xl", html);
    }
}
