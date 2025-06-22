using DoğrudanTeminLoggerAPI.Helpers;
using DoğrudanTeminLoggerAPI.Models;
using DoğrudanTeminLoggerAPI.Services.Abstract;
using DoğrudanTeminLoggerAPI.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DoğrudanTeminLoggerAPI.Services.Concrete
{
    public class PageService : IPageService
    {
        private readonly IMongoClient _mongoClient;
        private readonly MongoDbSettings _settings;
        private const int MaxDocsPerCollection = 150000;
        private const int MaxCollectionsPerDb = 25000;

        public PageService(IMongoClient mongoClient, IOptions<MongoDbSettings> opts)
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
                ? "DogrudanTeminPageLog"
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
                var count = (int)(await db.GetCollection<PageEntry>(name)
                    .CountDocumentsAsync(Builders<PageEntry>.Filter.Empty));
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

        public async Task PageAsync(PageEntryLogRequest request)
        {
            var db = GetDatabase();
            var colName = await GetActiveCollectionName(request.LogDateTime);
            var col = db.GetCollection<PageEntry>(colName);

            var entry = new PageEntry
            {
                Id = Guid.NewGuid(),
                PageUrl = request.PageUrl,
                LogDateTime = request.LogDateTime,
                UserId = request.UserId,
            };

            await col.InsertOneAsync(entry);
        }

        public async Task<IEnumerable<PageEntry>> QueryAsync(PageQueryParameters parameters)
        {
            var filterBuilder = Builders<PageEntry>.Filter;
            var filters = new List<FilterDefinition<PageEntry>>();

            if (parameters.From.HasValue)
                filters.Add(filterBuilder.Gte(e => e.LogDateTime, parameters.From.Value));
            if (parameters.To.HasValue)
                filters.Add(filterBuilder.Lt(e => e.LogDateTime, parameters.To.Value));
            if (parameters.UserId.HasValue)
                filters.Add(filterBuilder.Eq(e => e.UserId, parameters.UserId.Value));
            if (!string.IsNullOrEmpty(parameters.PageUrl))
                filters.Add(filterBuilder.Regex(e => e.PageUrl, new BsonRegularExpression(parameters.PageUrl, "i")));

            var filter = filters.Any() ? filterBuilder.And(filters) : filterBuilder.Empty;
            var results = new List<PageEntry>();

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
                    var col = db.GetCollection<PageEntry>(colName);
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
