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
    }
}
