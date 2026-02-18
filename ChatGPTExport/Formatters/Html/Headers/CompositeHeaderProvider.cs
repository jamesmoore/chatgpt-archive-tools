namespace ChatGPTExport.Formatters.Html.Headers
{
    internal class CompositeHeaderProvider(IEnumerable<IHeaderProvider> headerProviders) : IHeaderProvider
    {
        public string GetHeaders(HeaderInput htmlPage)
        {
            var headers = headerProviders.Select(p => p.GetHeaders(htmlPage));
            return string.Join(Environment.NewLine, headers).TrimEnd();
        }
    }
}
