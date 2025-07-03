using MongoDB.Bson.Serialization.Attributes;

namespace DoğrudanTeminLoggerAPI.Helpers
{
    public class LogEntryLogRequest
    {
        public DateTime LogDateTime { get; set; }
        public string? LogText { get; set; }
        public string? LogDescription { get; set; }
        public string? LogObject { get; set; }
        public string? LogIP { get; set; }
        public Guid? UserId { get; set; }
        public string? Token { get; set; }
    }

    public class LogQueryParameters
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public Guid? UserId { get; set; }
        public string? TextContains { get; set; }
        public string? DescriptionContains { get; set; }
        public string? IpContains { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 100;
    }

    public class PageEntryLogRequest
    {
        public string PageUrl { get; set; }
        public DateTime PageLogDateTime { get; set; }
        public Guid UserId { get; set; }
    }

    public class PageQueryParameters
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public Guid? UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 100;
        public string? PageUrl { get; set; }
    }
}
