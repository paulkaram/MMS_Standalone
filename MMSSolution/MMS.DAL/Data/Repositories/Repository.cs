using MMS.DAL.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories
{
	internal class Repository<TEntity> : IRepository<TEntity> where TEntity : class
	{
		protected readonly DbContext Context;

        public Repository(DbContext context)
        {
            Context = context;
        }

		public async Task AddAsync(TEntity entity)
		{
			await Context.Set<TEntity>().AddAsync(entity);
		}
		public void  Update(TEntity entity)
		{
			 Context.Set<TEntity>().Update(entity);
		}
		public async Task AddRangeAsync(IEnumerable<TEntity> entities)
		{
			await Context.Set<TEntity>().AddRangeAsync(entities);
		}
		public async Task<TEntity?> Find(object id)
		{
			return await Context.Set<TEntity>().FindAsync(id);
		}
		public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter)
		{
			return await Context.Set<TEntity>().FirstOrDefaultAsync(filter);
		}

		public async Task<IEnumerable<TEntity>> ListAsync()
		{
			return await Context.Set<TEntity>().AsNoTracking().ToListAsync();
		}

		public async Task<IEnumerable<TEntity>> ListAsync(Expression<Func<TEntity, bool>> filter)
		{
			return await Context.Set<TEntity>().AsNoTracking().Where(filter).ToListAsync();
		}
		public async Task<IEnumerable<TEntity>> ListWithTrackAsync(Expression<Func<TEntity, bool>> filter)
		{
			return await Context.Set<TEntity>().Where(filter).ToListAsync();
		}


		public async Task<IEnumerable<TEntity>> ListAsync<TOrderKey>(int page, int pageSize, Expression<Func<TEntity, bool>>? filter = null, Expression<Func<TEntity, TOrderKey>>? orderBy = null, bool isDescending = false)
		{
			var query = filter != null ? Context.Set<TEntity>().AsNoTracking().Where(filter) : Context.Set<TEntity>();

			if(orderBy != null)
			{
				query = isDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
			}

			return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
		}

		public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null)
		{
			return filter != null
				? await Context.Set<TEntity>().CountAsync(filter)
				: await Context.Set<TEntity>().CountAsync();
		}

		public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? filter = null)
		{
			return filter != null
				? await Context.Set<TEntity>().AnyAsync(filter)
				: await Context.Set<TEntity>().AnyAsync();
		}

		public void Remove(TEntity entity)
		{
			Context.Set<TEntity>().Remove(entity);
		}

		public void RemoveRange(IEnumerable<TEntity> entities)
		{
			Context.Set<TEntity>().RemoveRange(entities);
		}

	}
}
