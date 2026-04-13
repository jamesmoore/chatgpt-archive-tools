namespace ChatGPTExport.Formatters.Html.Template
{
    internal class TailwindHtmlFormatter : IHtmlFormatter
    {
        public string FormatHtmlPage(HtmlPage page, string pathPrefix)
        {
            var bodyModel = new
            {
                title = page.Title,
                body = page.Body.Select(fragment => new
                {
                    is_user = fragment.IsUser,
                    is_writing = fragment.IsWriting,
                    html = fragment.Html,
                }),
            };

            var model = new
            {
                title = page.Title,
                headers = page.Headers,
                body_html = TemplateRenderer.RenderTemplate("HtmlBody", bodyModel),
                pathprefix = pathPrefix,
                cssname = page.CssName,
            };

            return TemplateRenderer.RenderTemplate("Tailwind", model);
        }
    }
}
