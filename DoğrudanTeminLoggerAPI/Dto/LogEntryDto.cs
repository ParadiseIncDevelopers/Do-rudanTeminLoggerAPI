namespace DoğrudanTeminLoggerAPI.Dto
{
    public class LogEntryDto
    {
        public Guid Id { get; set; }
        public DateTime LogDateTime { get; set; }
        public string LogText { get; set; }
        public string LogDescription { get; set; }
        public string LogObject { get; set; }
        public string LogIP { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}
