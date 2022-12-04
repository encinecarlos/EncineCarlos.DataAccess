using System.Linq.Expressions;

namespace EncineCarlos.DataAccess.MongoDb
{
    public interface IMongoDbClient<TEntity, TId>
    {
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> predicate);
        Task CreateAsync(TEntity entity);
        Task<bool> UpdateAsync(TId id, TEntity entity);
        Task RemoveAsync(TId id);
    }
}
