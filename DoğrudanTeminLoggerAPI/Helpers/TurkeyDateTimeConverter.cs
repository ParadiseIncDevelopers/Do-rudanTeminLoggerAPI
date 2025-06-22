using System.Text.Json;
using System.Text.Json.Serialization;

namespace DoğrudanTeminLoggerAPI.Helpers
{
    public class TurkeyDateTimeConverter : JsonConverter<DateTime>
    {
        private static readonly TimeZoneInfo TurkeyTimeZone;

        static TurkeyDateTimeConverter()
        {
            // Windows: "Turkey Standard Time", Linux/macOS: "Europe/Istanbul"
            try
            {
                TurkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                TurkeyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
            }
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Parse as UTC then convert to local
            var str = reader.GetString();
            if (string.IsNullOrEmpty(str))
                return default;
            // Attempt parse
            if (DateTime.TryParse(str, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var utc))
            {
                // Convert from UTC to Turkey time
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), TurkeyTimeZone);
            }
            return DateTime.Parse(str);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Assume incoming value is UTC or Unspecified in UTC
            DateTime utcValue = value.Kind == DateTimeKind.Utc
                ? value
                : TimeZoneInfo.ConvertTimeToUtc(value);

            // Convert UTC to Turkey time
            var local = TimeZoneInfo.ConvertTimeFromUtc(utcValue, TurkeyTimeZone);

            // Write in ISO 8601 format
            writer.WriteStringValue(local.ToString("o"));
        }
    }
}
