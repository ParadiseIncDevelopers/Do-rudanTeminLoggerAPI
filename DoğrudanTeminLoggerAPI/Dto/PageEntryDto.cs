namespace DoğrudanTeminLoggerAPI.Dto
{
    public class PageEntryDto
    {
        public Guid Id { get; set; }
        public DateTime PageLogDateTime { get; set; }
        public string PageUrl { get; set; }
        public Guid UserId { get; set; }
    }
}
