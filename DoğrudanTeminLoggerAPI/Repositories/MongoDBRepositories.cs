using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace DoğrudanTeminLoggerAPI.Repositories
{
    public class MongoDBRepository<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;

        public MongoDBRepository(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<T>(collectionName);
        }

        public void Insert(T document)
        {
            _collection.InsertOne(document);
        }

        public async Task InsertAsync(T document)
        {
            await _collection.InsertOneAsync(document);
        }

        public T GetById(Guid? id)
        {
            return _collection.Find(Builders<T>.Filter.Eq("_id", id.ToString())).FirstOrDefault();
        }

        public async Task<T> GetByIdAsync(Guid? id)
        {
            return await _collection.Find(Builders<T>.Filter.Eq("_id", id.ToString())).FirstOrDefaultAsync();
        }

        public IEnumerable<T> GetAll()
        {
            return _collection.AsQueryable();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _collection.AsQueryable().ToListAsync();
        }

        public void Update(Guid id, T document)
        {
            _collection.ReplaceOne(Builders<T>.Filter.Eq("_id", id.ToString()), document);
        }

        public async Task UpdateAsync(Guid id, T document)
        {
            await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", id.ToString()), document);
        }

        public void Delete(Guid id)
        {
            _collection.DeleteOne(Builders<T>.Filter.Eq("_id", id.ToString()));
        }

        public async Task DeleteAsync(Guid id)
        {
            await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id.ToString()));
        }

        public IEnumerable<T> GetMultiple(List<Guid> idList)
        {
            var filter = Builders<T>.Filter.In("_id", idList);
            return _collection.Find(filter).ToList();
        }

        public async Task<IEnumerable<T>> GetMultipleAsync(List<Guid> idList)
        {
            var filter = Builders<T>.Filter.In("_id", idList);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(FilterDefinition<T> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetFilteredAsync(FilterDefinition<T> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }
    }
}
