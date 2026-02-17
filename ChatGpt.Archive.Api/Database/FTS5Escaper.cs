using System.Text.RegularExpressions;

namespace ChatGpt.Archive.Api.Database
{
    public partial class FTS5Escaper
    {
        private const int MaxSearchQueryLength = 512;
        private static readonly Regex BooleanOperatorRegex = BooleanOperatorCompiledRegex();
        private static readonly Regex NearOperatorRegex = NearOperatorCompiledRegex();

        public string QuoteAsLiteral(string query)
        {
            return "\"" + query.Replace("\"", "\"\"") + "\"";
        }

        public string EscapeFts5Query(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return "\"\"";
            }

            var trimmed = query.Trim();
            if (trimmed.Length > MaxSearchQueryLength)
            {
                throw new ArgumentException($"Search query is too long (max {MaxSearchQueryLength} characters).", nameof(query));
            }

            var hasOperators = BooleanOperatorRegex.IsMatch(trimmed) || NearOperatorRegex.IsMatch(trimmed);
            if (hasOperators && IsLikelyValidFtsExpression(trimmed))
            {
                return trimmed;
            }

            return QuoteAsLiteral(trimmed);
        }

        private static bool IsLikelyValidFtsExpression(string query)
        {
            var parenDepth = 0;
            var inQuotes = false;

            for (var i = 0; i < query.Length; i++)
            {
                var ch = query[i];

                if (ch == '"')
                {
                    if (inQuotes && i + 1 < query.Length && query[i + 1] == '"')
                    {
                        i++;
                        continue;
                    }

                    inQuotes = !inQuotes;
                    continue;
                }

                if (inQuotes)
                {
                    continue;
                }

                if (ch == '(')
                {
                    parenDepth++;
                }
                else if (ch == ')')
                {
                    parenDepth--;
                    if (parenDepth < 0)
                    {
                        return false;
                    }
                }
            }

            return !inQuotes && parenDepth == 0;
        }


        [GeneratedRegex(@"\b(?:AND|OR|NOT)\b")]
        private static partial Regex BooleanOperatorCompiledRegex();
        [GeneratedRegex(@"\bNEAR\s*\(")]
        private static partial Regex NearOperatorCompiledRegex();
    }
}
