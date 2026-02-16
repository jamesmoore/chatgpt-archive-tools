using System.Globalization;
using System.Text.Json;

namespace ChatGPTExport
{
    public static class ExtensionMethods
    {
        public static DateTimeOffset ToDateTimeOffset(this decimal d)
        {
            // Convert to total milliseconds
            // If the value is < 10^10, assume it's in seconds, so scale it
            var millis = d < 1_000_000_000_0m ? d * 1000 : d;

            // Truncate to long — DateTimeOffset doesn't support sub-millisecond precision
            return DateTimeOffset.FromUnixTimeMilliseconds((long)millis);
        }

        public static bool IsValidJson(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();

            if (!(input.StartsWith("{") && input.EndsWith("}")) &&
                !(input.StartsWith("[") && input.EndsWith("]")))
            {
                return false;
            }

            try
            {
                using var doc = JsonDocument.Parse(input);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
            catch
            {
                return false; // optionally handle or log other exceptions
            }
        }

        public static IList<int> GetRenderedElementIndexes(this string input)
        {

            var realList = new List<int>();
            var textElements = StringInfo.GetTextElementEnumerator(input);
            int count = 0;
            while (textElements.MoveNext())
            {
                string element = textElements.GetTextElement();
                realList.Add(count);
                count += element.GetRealElementWidth();
            }

            return realList;
        }

        public static int GetRealElementWidth(this string element)
        {
            const int VariationSelector16 = 0xFE0F;

            // Check for Regional Indicator pairs (flag emoji) - count as 3
            // This must be checked first before other processing
            var enumerator = element.EnumerateRunes().GetEnumerator();
            if (enumerator.MoveNext())
            {
                var firstRune = enumerator.Current;
                if (enumerator.MoveNext())
                {
                    var secondRune = enumerator.Current;
                    if (!enumerator.MoveNext() && // Only two runes
                        firstRune.Value >= 0x1F1E6 && firstRune.Value <= 0x1F1FF &&
                        secondRune.Value >= 0x1F1E6 && secondRune.Value <= 0x1F1FF)
                    {
                        return 3;
                    }
                }
            }

            // Check if element contains Variation Selector 16 and count runes
            bool hasVS16 = false;
            int runeCount = 0;
            foreach (var rune in element.EnumerateRunes())
            {
                if (rune.Value == VariationSelector16)
                {
                    hasVS16 = true;
                }
                else
                {
                    runeCount++;
                }
            }

            if (hasVS16)
            {
                // Complex emoji with VS16 (more than just char + VS16) - count as 2
                if (element.Length > 2)
                {
                    return 2;
                }
                // Simple char + VS16 - return the count without VS16
                return runeCount;
            }

            // For single surrogate pair emoji (e.g., 🚗), count as 2 units
            // This matches how the external system (ChatGPT API) counts positions
            if (runeCount == 1 && element.Length == 2)
            {
                return 2;
            }

            // For complex sequences, return the rune count
            return runeCount;
        }
    }
}
