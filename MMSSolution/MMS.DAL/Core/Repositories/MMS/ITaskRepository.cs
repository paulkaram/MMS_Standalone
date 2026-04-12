using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface ITaskRepository: IRepository<Models.MMS.Task>
    {
        Task<List<Models.MMS.Task>> ListIncludeAllAsync<TOrderKey>(int page, int pageSize, Expression<Func<DAL.Models.MMS.Task, bool>>? filter = null, Expression<Func<Models.MMS.Task, TOrderKey>>? orderBy = null, bool isDescending = false);
        Task<List<Models.MMS.Task>> ListIncludeUserAsync(Expression<Func<Models.MMS.Task, bool>> filter);
        Task<List<Models.MMS.Task>> ListIncludeUserAndStatusAsync(Expression<Func<Models.MMS.Task, bool>> filter);

		// Performance optimized batch methods
		Task<List<T>> GetRecentActivitiesAsync<T>(string userId, int count, Expression<Func<Models.MMS.Task, T>> selector);
    }
}
