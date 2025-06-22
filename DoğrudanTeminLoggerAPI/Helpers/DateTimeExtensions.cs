namespace DoğrudanTeminLoggerAPI.Helpers
{
    public static class DateTimeExtensions
    {
        private static readonly TimeZoneInfo TurkeyZone =
            TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");

        public static DateTime ToTurkeyTime(this DateTime utcDateTime)
        {
            // Kind’ı kesinleştirelim
            var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, TurkeyZone);
        }
    }
}
