using Markdig;

namespace ChatGPTExport.Formatters.Html.Template
{
    internal class TailwindHtmlFormatter : IHtmlFormatter
    {
        public void ApplyMarkdownPipelineBuilder(MarkdownPipelineBuilder markdownPipelineBuilder)
        {
        }

        public string FormatHtmlPage(HtmlPage page)
        {
            var model = new
            {
                title = page.Title,
                headers = page.Headers,
                body = page.Body.Select(fragment => new
                {
                    is_user = fragment.IsUser,
                    html = fragment.Html
                })
            };

            return TemplateRenderer.RenderTemplate("Tailwind", model);
        }
    }
}
