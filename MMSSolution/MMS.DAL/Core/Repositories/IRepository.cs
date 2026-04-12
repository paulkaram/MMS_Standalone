using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
		Task<TEntity?> Find(object id);
		Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter);
        Task<IEnumerable<TEntity>> ListAsync();
        Task<IEnumerable<TEntity>> ListAsync(Expression<Func<TEntity, bool>> filter);
        Task<IEnumerable<TEntity>> ListWithTrackAsync(Expression<Func<TEntity, bool>> filter);
        Task<IEnumerable<TEntity>> ListAsync<TOrderKey>(int page, int pageSize, Expression<Func<TEntity, bool>>? filter = null, Expression<Func<TEntity, TOrderKey>>? orderBy = null, bool isDescending = false);
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? filter = null);

        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
		
	}
}
