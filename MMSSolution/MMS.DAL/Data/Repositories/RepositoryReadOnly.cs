using MMS.DAL.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories
{
	internal class RepositoryReadOnly<TEntity> : IRepositoryReadOnly<TEntity> where TEntity : class
	{
		protected readonly DbContext Context;

        public RepositoryReadOnly(DbContext context)
        {
            Context = context;
        }

		public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter)
		{
			return await Context.Set<TEntity>().FirstOrDefaultAsync(filter);
		}

		public async Task<IEnumerable<TEntity>> ListAsync()
		{
			return await Context.Set<TEntity>().ToListAsync();
		}

		public async Task<IEnumerable<TEntity>> ListAsync(Expression<Func<TEntity, bool>> filter)
		{
			return await Context.Set<TEntity>().Where(filter).ToListAsync();
		}

		public async Task<IEnumerable<TEntity>> ListAsync<TOrderKey>(int page, int pageSize, Expression<Func<TEntity, bool>>? filter = null, Expression<Func<TEntity, TOrderKey>>? orderBy = null, bool isDescending = false)
		{
			var query = filter != null ? Context.Set<TEntity>().Where(filter) : Context.Set<TEntity>();

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

	}
}
