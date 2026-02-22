namespace ChatGPTExport.Formatters.Html.Template
{
    internal class TailwindHtmlFormatter : IHtmlFormatter
    {
        public string FormatHtmlPage(HtmlPage page, string pathPrefix)
        {
            var model = new
            {
                title = page.Title,
                headers = page.Headers,
                body = page.Body.Select(fragment => new
                {
                    is_user = fragment.IsUser,
                    html = fragment.Html,
                }),
                pathprefix = pathPrefix,
                cssname = page.CssName,
            };

            return TemplateRenderer.RenderTemplate("Tailwind", model);
        }
    }
}
