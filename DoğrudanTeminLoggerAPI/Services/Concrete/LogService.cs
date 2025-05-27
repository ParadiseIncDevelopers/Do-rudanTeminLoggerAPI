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
            var baseName = _settings.DatabaseBaseName;
            var dbNames = GetAllDatabaseNames()
                .Where(n => n.StartsWith(baseName))
                .OrderBy(n => n)
                .ToList();

            foreach (var name in dbNames)
            {
                var db = _mongoClient.GetDatabase(name);
                if (db.ListCollectionNames().ToList().Count < MaxCollectionsPerDb)
                    return name;
            }
            // create next
            var nextIndex = dbNames.Count;
            return $"{baseName}_{nextIndex:D4}";
        }

        private IMongoDatabase GetDatabase()
        {
            var dbName = GetActiveDatabaseName();
            return _mongoClient.GetDatabase(dbName);
        }

        private async Task<string> GetActiveCollectionName(DateTime date)
        {
            var db = GetDatabase();
            var prefix = $"logs_{date:yyyyMMdd}";
            var colNames = (await db.ListCollectionNames().ToListAsync())
                .Where(n => n.StartsWith(prefix))
                .OrderBy(n => n)
                .ToList();

            foreach (var name in colNames)
            {
                var count = (int)(await db.GetCollection<LogEntry>(name)
                    .CountDocumentsAsync(FilterDefinition<LogEntry>.Empty));
                if (count < MaxDocsPerCollection)
                    return name;
            }
            // create next
            var next = colNames.Count;
            return $"{prefix}_{next:D4}";
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
