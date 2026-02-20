namespace ChatGPTExport.Decoders
{
    public sealed class CodePointIndexMap
    {
        // Maps code point index -> UTF16 index
        private readonly int[] _map;

        public CodePointIndexMap(string text)
        {
            // First count runes (code points)
            int runeCount = text.EnumerateRunes().Count();

            _map = new int[runeCount + 1];

            int codePointIndex = 0;
            int utf16Index = 0;

            foreach (var rune in text.EnumerateRunes())
            {
                _map[codePointIndex] = utf16Index;

                utf16Index += rune.Utf16SequenceLength;
                codePointIndex++;
            }

            // Sentinel: map last code point index to end of string
            _map[codePointIndex] = utf16Index;
        }

        public int ToUtf16Index(int codePointIndex)
        {
            if (codePointIndex < 0 || codePointIndex >= _map.Length)
                throw new ArgumentOutOfRangeException(nameof(codePointIndex));

            return _map[codePointIndex];
        }
    }

}
