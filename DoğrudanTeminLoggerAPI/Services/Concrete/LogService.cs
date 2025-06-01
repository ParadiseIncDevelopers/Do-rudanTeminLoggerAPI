using DoğrudanTeminLoggerAPI.Helpers;
using DoğrudanTeminLoggerAPI.Models;
using DoğrudanTeminLoggerAPI.Services.Abstract;
using DoğrudanTeminLoggerAPI.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace DoğrudanTeminLoggerAPI.Services.Concrete
{
    public class LogService : ILogService
    {
        private readonly IMongoClient _mongoClient;
        private readonly MongoDbSettings _settings;
        private const int MaxDocsPerCollection = 50000;
        private const int MaxCollectionsPerDb = 2500;

        public LogService(IMongoClient mongoClient, IOptions<MongoDbSettings> opts)
        {
            _mongoClient = mongoClient;
            _settings = opts.Value;
        }

        private IEnumerable<string> GetAllDatabaseNames()
            => _mongoClient.ListDatabaseNames().ToList();

        private string GetActiveDatabaseName()
        {
            // Eğer ayarlardan bir değer gelmediyse varsayılan baseName'i atıyoruz
            var baseName = string.IsNullOrWhiteSpace(_settings.DatabaseBaseName)
                ? "DogrudanTeminLog"
                : _settings.DatabaseBaseName;

            // BaseName ile başlayan mevcut veritabanı isimlerini alıyoruz
            var dbNames = GetAllDatabaseNames()
                .Where(n => n.StartsWith(baseName))
                .OrderBy(n => n)
                .ToList();

            // Mevcut veritabanlarında hâlâ boş yer varsa (2500 koleksiyon alt sınırı), döndür
            foreach (var name in dbNames)
            {
                var db = _mongoClient.GetDatabase(name);
                var collectionCount = db.ListCollectionNames().ToList().Count;
                if (collectionCount < MaxCollectionsPerDb)
                {
                    return name;
                }
            }

            // Eğer hiç database yoksa veya hepsinde 2500 koleksiyon doluysa yeni bir tane oluştur
            // Burada 1-based suffix kullanıyoruz, ilk veritabanı "DogrudanTemin_0001" olacak
            var nextIndex = dbNames.Count + 1;
            return $"{baseName}_{nextIndex:D4}";
        }

        private IMongoDatabase GetDatabase()
        {
            var dbName = GetActiveDatabaseName();
            return _mongoClient.GetDatabase(dbName);
        }

        private async Task<string> GetActiveCollectionName(DateTime date)
        {
            // Prefix sürekli aynı kalacak, suffix kısmını manuel artıracağız
            const string collPrefix = "TeminColl";

            var db = GetDatabase(); // yukarıdaki GetActiveDatabaseName kullanılarak seçilen DB
            var allCollNames = (await db.ListCollectionNames().ToListAsync())
                .Where(n => n.StartsWith(collPrefix))
                .OrderBy(n => n)
                .ToList();

            // Öncelikle, mevcut "TeminColl_XXXX" adlarına bakıp hala 50.000 belge barındırmayan varsa onu döndür.
            foreach (var name in allCollNames)
            {
                var count = (int)(await db.GetCollection<LogEntry>(name)
                    .CountDocumentsAsync(Builders<LogEntry>.Filter.Empty));
                if (count < MaxDocsPerCollection)
                {
                    return name;
                }
            }

            // Eğer hiç koleksiyon yoksa ya da hepsi 50.000 sınırına ulaşmışsa yeni bir tanesini oluştur
            // 1-based suffix: "TeminColl_0001"
            var nextIndex = allCollNames.Count + 1;
            return $"{collPrefix}_{nextIndex:D4}";
        }

        public async Task LogAsync(LogEntryLogRequest request)
        {
            var db = GetDatabase();
            var colName = await GetActiveCollectionName(request.LogDateTime);
            var col = db.GetCollection<LogEntry>(colName);

            var entry = new LogEntry
            {
                Id = Guid.NewGuid(),
                LogDateTime = request.LogDateTime,
                LogText = request.LogText,
                LogDescription = request.LogDescription,
                LogObjectJson = JsonConvert.SerializeObject(request.LogObject),
                LogIP = request.LogIP,
                UserId = request.UserId,
                Token = request.Token
            };

            await col.InsertOneAsync(entry);
        }

        public async Task<IEnumerable<LogEntry>> QueryAsync(LogQueryParameters parameters)
        {
            var filterBuilder = Builders<LogEntry>.Filter;
            var filters = new List<FilterDefinition<LogEntry>>();

            if (parameters.From.HasValue)
                filters.Add(filterBuilder.Gte(e => e.LogDateTime, parameters.From.Value));
            if (parameters.To.HasValue)
                filters.Add(filterBuilder.Lt(e => e.LogDateTime, parameters.To.Value));
            if (parameters.UserId.HasValue)
                filters.Add(filterBuilder.Eq(e => e.UserId, parameters.UserId.Value));
            if (!string.IsNullOrEmpty(parameters.TextContains))
                filters.Add(filterBuilder.Regex(e => e.LogText, new BsonRegularExpression(parameters.TextContains, "i")));
            if (!string.IsNullOrEmpty(parameters.DescriptionContains))
                filters.Add(filterBuilder.Regex(e => e.LogDescription, new BsonRegularExpression(parameters.DescriptionContains, "i")));
            if (!string.IsNullOrEmpty(parameters.IpContains))
                filters.Add(filterBuilder.Regex(e => e.LogIP, new BsonRegularExpression(parameters.IpContains, "i")));

            var filter = filters.Any() ? filterBuilder.And(filters) : filterBuilder.Empty;
            var results = new List<LogEntry>();

            // scan all DBs
            var dbNames = GetAllDatabaseNames()
                .Where(n => n.StartsWith(_settings.DatabaseBaseName));
            foreach (var dbName in dbNames)
            {
                var db = _mongoClient.GetDatabase(dbName);
                var colNames = (await db.ListCollectionNames().ToListAsync())
                    .Where(n => n.StartsWith("logs_"));

                foreach (var colName in colNames)
                {
                    var col = db.GetCollection<LogEntry>(colName);
                    var docs = await col.Find(filter)
                        .Skip((parameters.Page - 1) * parameters.PageSize)
                        .Limit(parameters.PageSize)
                        .ToListAsync();
                    results.AddRange(docs);
                }
            }
            return results;
        }
    }
}
