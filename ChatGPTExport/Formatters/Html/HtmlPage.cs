namespace ChatGPTExport.Formatters.Html
{

    public record HtmlPage(
        string Title,
        IEnumerable<string> Headers,
        IEnumerable<HtmlFragment> Body,
        string CssName);

    public record HtmlFragment(
        bool IsUser,
        string Html,
        bool HasCode,
        bool HasMath,
        bool HasImage,
        IReadOnlyCollection<string> Languages);
}
