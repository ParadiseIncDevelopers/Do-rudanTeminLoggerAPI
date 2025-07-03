using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DoğrudanTeminLoggerAPI.Models
{
    public class LogEntry
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LogDateTime { get; set; }
        public string LogText { get; set; }
        public string LogDescription { get; set; }
        public string LogObjectJson { get; set; }
        public string LogIP { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid? UserId { get; set; }
        public string Token { get; set; }
    }
}
