using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Scriban;

namespace ChatGPTExport.Formatters.Html.Template
{
    internal static class TemplateRenderer
    {
        private static readonly Assembly Assembly = typeof(TemplateRenderer).Assembly;
        private static readonly ConcurrentDictionary<string, Scriban.Template> TemplateCache = new();

        public static string RenderTemplate(string templateName, object model)
        {
            var template = TemplateCache.GetOrAdd(templateName, LoadTemplate);
            
            try
            {
                return template.Render(model);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to render template '{templateName}': {ex.Message}", ex);
            }
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
            
            try
            {
                return Scriban.Template.Parse(templateContent);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse template '{templateName}': {ex.Message}", ex);
            }
        }
    }
}
