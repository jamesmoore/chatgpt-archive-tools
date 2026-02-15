namespace ChatGPTExport.Formatters.Html
{

    public record HtmlPage(string Title, IEnumerable<string> Headers, IEnumerable<HtmlFragment> Body)
    {
    }


    public record HtmlFragment(
        bool IsUser,
        string Html,
        bool HasCode,
        bool HasMath,
        bool HasImage,
        IReadOnlyCollection<string> Languages)
    {
        public override string ToString() => Html;
    }
}
