namespace ChatGPTExport.Formatters.Html.Headers
{
    public record HeaderInput(IEnumerable<HtmlFragment> HtmlFragments, Dictionary<string, string> MetaHeaders)
    {
        public bool HasCode => HtmlFragments.Any(p => p.HasCode);
        public bool HasMath => HtmlFragments.Any(p => p.HasMath);
        public bool HasImage => HtmlFragments.Any(_ => _.HasImage);
        public IReadOnlyCollection<string> Languages => HtmlFragments.SelectMany(p => p.Languages).Distinct().ToList();
    }
}
