using Markdig;

namespace ChatGPTExport.Formatters.Html
{
    internal interface IHtmlFormatter
    {
        string FormatHtmlPage(HtmlPage page, string pathPrefix);
    }
}