using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace EncineCarlos.DataAccess.MongoDb
{
    public class MongoDbClient<TEntity, TId> : IMongoDbClient<TEntity, TId> where TEntity : BaseEntity<TId>
    {
        private MongoClient Client { get; }
        private IMongoCollection<TEntity> Collection { get; }
        public MongoDbClient(IOptions<MongoDbSettings> config)
        {
            ArgumentNullException.ThrowIfNull(config.Value);

            try
            {
                var name = typeof(TEntity).Name;

                BsonClassMap.RegisterClassMap<TEntity>(cm => 
                {
                    cm.AutoMap();
                    cm.MapIdMember(x => x.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
                });
                MongoUrl mongoUrl = new MongoUrl(config.Value.ConnectionString);
                var mongoClientsettings = MongoClientSettings.FromUrl(mongoUrl);
                Client = new MongoClient(mongoClientsettings);

                Collection = Client.GetDatabase(config.Value.Database).GetCollection<TEntity>(name);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IClientSessionHandle StartSettion()
        {
            return Client.StartSession();
        }

        public async Task CreateAsync(TEntity entity)
        {
            await Collection.InsertOneAsync(entity);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate) 
            => await ((await Collection.FindAsync(predicate)).ToListAsync());

        public async Task<TEntity> getSingleAsync(Expression<Func<TEntity, bool>> predicate)
            => await ((await Collection.FindAsync(predicate)).FirstOrDefaultAsync());

        public async Task RemoveAsync(TId id)
            => await Collection.DeleteOneAsync(Builders<TEntity>.Filter.Eq("_id", id));

        public async Task<bool> UpdateAsync(TId id, TEntity entity)
        {
            entity.Id = id;
            var documentChanges = BsonDocument.Parse(entity.ToJson());
            var filter = Builders<TEntity>.Filter.Eq("_id", id);
            var firstElement = documentChanges.FirstOrDefault();

            if(getSingleAsync(i => i.Id.Equals(id)) != null)
            {
                var updates = Builders<TEntity>.Update.Set(firstElement.Name, firstElement.Value);

                foreach (var item in documentChanges)
                {
                    if(!item.Value.IsBsonNull && item.Name != "_id")
                    {
                        updates = updates.Set(item.Name, item.Value);
                    }
                }

                var result = Collection.UpdateOneAsync(filter, updates);

                return await Task.FromResult(result.IsCompletedSuccessfully);
            } else
            {
                return false;
            }
        }
    }
}