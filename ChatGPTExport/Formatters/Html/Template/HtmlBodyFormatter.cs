namespace ChatGPTExport.Formatters.Html.Template
{
    internal class HtmlBodyFormatter : IHtmlFormatter
    {
        public string FormatHtmlPage(HtmlPage page, string pathPrefix)
        {
            var model = new
            {
                title = page.Title,
                body = page.Body.Select(fragment => new
                {
                    is_user = fragment.IsUser,
                    is_writing = fragment.IsWriting,
                    html = fragment.Html,
                }),
            };

            return TemplateRenderer.RenderTemplate("HtmlBody", model);
        }
    }
}
