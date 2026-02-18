using ChatGpt.Archive.Api.Database;

namespace ChatGpt.Archive.Api.Test
{
    public class FTS5EscaperTests
    {
        private readonly FTS5Escaper escaper = new();

        [Fact]
        public void QuoteAsLiteral_DoublesInnerQuotes_AndWrapsInQuotes()
        {
            var result = escaper.QuoteAsLiteral("say \"hello\"");

            Assert.Equal("\"say \"\"hello\"\"\"", result);
        }

        [Fact]
        public void EscapeFts5Query_WhitespaceOnly_ReturnsEmptyQuotedLiteral()
        {
            var result = escaper.EscapeFts5Query("   ");

            Assert.Equal("\"\"", result);
        }

        [Fact]
        public void EscapeFts5Query_PlainText_QuotesTrimmedInput()
        {
            var result = escaper.EscapeFts5Query("  hello world  ");

            Assert.Equal("\"hello world\"", result);
        }

        [Fact]
        public void EscapeFts5Query_ValidBooleanExpression_ReturnsAsIs()
        {
            var result = escaper.EscapeFts5Query("apple AND (banana OR \"fruit salad\")");

            Assert.Equal("apple AND (banana OR \"fruit salad\")", result);
        }

        [Fact]
        public void EscapeFts5Query_InvalidBooleanExpression_QuotesInput()
        {
            var result = escaper.EscapeFts5Query("apple AND (banana OR cherry");

            Assert.Equal("\"apple AND (banana OR cherry\"", result);
        }

        [Fact]
        public void EscapeFts5Query_ValidNearExpression_ReturnsAsIs()
        {
            var result = escaper.EscapeFts5Query("NEAR(one two)");

            Assert.Equal("NEAR(one two)", result);
        }

        [Fact]
        public void EscapeFts5Query_TooLong_ThrowsArgumentException()
        {
            var query = new string('a', 513);

            var ex = Assert.Throws<ArgumentException>(() => escaper.EscapeFts5Query(query));

            Assert.Equal("query", ex.ParamName);
        }
    }
}
