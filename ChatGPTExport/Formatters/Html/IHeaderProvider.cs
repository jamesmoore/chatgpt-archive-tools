using ChatGPTExport.Formatters.Html.Headers;

namespace ChatGPTExport.Formatters.Html
{
    internal interface IHeaderProvider
    {
        string GetHeaders(HeaderInput htmlPage);
    }
}