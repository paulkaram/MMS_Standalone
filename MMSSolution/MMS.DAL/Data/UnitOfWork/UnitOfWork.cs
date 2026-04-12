using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.UnitOfWork;

namespace MMS.DAL.Data.UnitOfWork
{
	internal class UnitOfWork : IUnitOfWork
	{
		protected readonly DbContext _dbContext;

        public UnitOfWork(DbContext dbContext)
        {
			_dbContext = dbContext;
		}

        public async Task<int> SaveChangesAsync()
		{
			return await _dbContext.SaveChangesAsync();
		}

	}
}
