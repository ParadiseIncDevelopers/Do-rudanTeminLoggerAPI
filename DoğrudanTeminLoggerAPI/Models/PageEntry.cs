using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DoğrudanTeminLoggerAPI.Models
{
    public class PageEntry
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public DateTime LogDateTime { get; set; }
        public string PageUrl { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }
    }
}
