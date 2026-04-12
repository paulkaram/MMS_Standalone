using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class TasksRepository : Repository<Models.MMS.Task>, ITaskRepository
    {
        MmsContext ContextAsMMSContext => (Context as MmsContext)!;
        public TasksRepository(DbContext context) : base(context)
        {
        }


        public async Task<List<Models.MMS.Task>> ListIncludeAllAsync<TOrderKey>(int page, int pageSize, Expression<Func<Models.MMS.Task, bool>>? filter, Expression<Func<Models.MMS.Task, TOrderKey>>? orderBy, bool isDescending)
        {
            var query = filter != null ? Context.Set<Models.MMS.Task>()
               .Include(x => x.Type)
               .Include(x => x.Status)
               .Include(x => x.Meeting)
               .Where(filter) : Context.Set<Models.MMS.Task>();

            if (orderBy != null)
            {
                query = isDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
            }

            return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<List<Models.MMS.Task>> ListIncludeUserAsync(Expression<Func<Models.MMS.Task, bool>> filter)
        {
            return await ContextAsMMSContext.Tasks.Where(filter).Include(x => x.User).ToListAsync();
        }
		public async Task<List<Models.MMS.Task>> ListIncludeUserAndStatusAsync(Expression<Func<Models.MMS.Task, bool>> filter)
		{
			return await ContextAsMMSContext.Tasks
				.Where(filter)
				.Include(x => x.User)
				.Include(x => x.Status)
				.Include(x => x.MeetingNotes)
				.Include(x => x.Attachment)
				.ToListAsync();
		}

		// Performance optimized batch methods

		/// <summary>
		/// Get recent task activities with projection applied at database level
		/// </summary>
		public async Task<List<T>> GetRecentActivitiesAsync<T>(string userId, int count, Expression<Func<Models.MMS.Task, T>> selector)
		{
			return await ContextAsMMSContext.Tasks.AsNoTracking()
				.Include(t => t.Meeting)
				.Where(t => t.UserId == userId)
				.OrderByDescending(t => t.CreatedDate)
				.Take(count)
				.Select(selector)
				.ToListAsync();
		}
	}
}
