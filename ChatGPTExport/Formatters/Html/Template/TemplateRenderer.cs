using System.IO;
using System.Reflection;
using Scriban;

namespace ChatGPTExport.Formatters.Html.Template
{
    internal static class TemplateRenderer
    {
        private static readonly Assembly Assembly = typeof(TemplateRenderer).Assembly;

        public static string RenderTemplate(string templateName, object model)
        {
            var template = LoadTemplate(templateName);
            return template.Render(model);
        }

        private static Scriban.Template LoadTemplate(string templateName)
        {
            var resourceName = $"ChatGPTExport.Formatters.Html.Templates.{templateName}.scriban";
            
            using var stream = Assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new InvalidOperationException($"Template '{resourceName}' not found in embedded resources.");
            }

            using var reader = new StreamReader(stream);
            var templateContent = reader.ReadToEnd();
            
            return Scriban.Template.Parse(templateContent);
        }
    }
}
