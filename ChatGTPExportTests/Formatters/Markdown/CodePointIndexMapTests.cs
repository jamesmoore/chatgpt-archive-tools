namespace ChatGTPExportTests.Formatters.Markdown
{
    public class CodePointIndexMapTests
    {
        [Fact]
        public void CodePointIndexMapTest()
        {
            var testString = "Hello, 👋🌍!"; // Contains ASCII and multi-code-unit emojis

            var map = new ChatGPTExport.Formatters.Markdown.CodePointIndexMap(testString);

            Assert.Equal(0, map.ToUtf16Index(0)); // 'H'
            Assert.Equal(7, map.ToUtf16Index(7)); // '👋' starts at UTF-16 index 7
            Assert.Equal(9, map.ToUtf16Index(8)); // '🌍' starts at UTF-16 index 9
            Assert.Equal(11, map.ToUtf16Index(9)); // '!' starts at UTF-16 index 11
        }

        [Fact]
        public void CodePointIndexMap_OutOfRangeTest()
        {
            var testString = "Test";
            var map = new ChatGPTExport.Formatters.Markdown.CodePointIndexMap(testString);
            Assert.Throws<ArgumentOutOfRangeException>(() => map.ToUtf16Index(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => map.ToUtf16Index(5)); // Only 4 code points
        }   
    }
}
