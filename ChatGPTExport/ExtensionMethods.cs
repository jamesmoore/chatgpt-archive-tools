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

            var runes = element.EnumerateRunes().ToList();

            // Special case: Regional Indicator pairs (flag emoji) - count as 3
            if (runes.Count == 2 &&
                runes[0].Value >= 0x1F1E6 && runes[0].Value <= 0x1F1FF &&
                runes[1].Value >= 0x1F1E6 && runes[1].Value <= 0x1F1FF)
            {
                return 3;
            }

            // Check if element contains Variation Selector 16
            bool hasVS16 = runes.Any(r => r.Value == VariationSelector16);

            if (hasVS16)
            {
                // Complex emoji with VS16 (more than just char + VS16) - count as 2
                if (element.Length > 2)
                {
                    return 2;
                }
                // Simple char + VS16 - don't count the VS16
                return runes.Count(r => r.Value != VariationSelector16);
            }

            // For surrogate pairs (emoji without VS16), count as 2 units
            // This matches how the external system (ChatGPT API) counts positions
            if (element.Length == 2 && char.IsSurrogatePair(element, 0))
            {
                return 2;
            }

            // For complex sequences, count the number of runes (Unicode scalar values)
            return runes.Count;
        }
    }
}
