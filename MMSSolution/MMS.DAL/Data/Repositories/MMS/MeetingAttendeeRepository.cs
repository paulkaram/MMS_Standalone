using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class MeetingAttendeeRepository : Repository<MeetingAttendee>, IMeetingAttendeeRepository
    {
		MmsContext ContextAsMMSContext => (Context as MmsContext)!;
		public MeetingAttendeeRepository(DbContext context) : base(context)
        {
        }

		public async Task<List<MeetingAttendee>> ListIncludeUserAsync(Expression<Func<MeetingAttendee, bool>> filter)
		{
			return await ContextAsMMSContext.MeetingAttendees.Where(filter).Include(x=>x.User).ToListAsync();
		}
		public async Task<List<MeetingAttendee>> ListIncludeUserAndMeetingAndComitteeAsync(Expression<Func<MeetingAttendee, bool>> filter, int page,int pageSize)
		{
			return await ContextAsMMSContext.MeetingAttendees.Where(filter).Include(x => x.User)
				.Include(x=>x.Meeting).ThenInclude(x=>x.Committee)
				.Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
		}

		// Performance optimized batch methods

		/// <summary>
		/// Get attendance statistics grouped by committee IDs in a single query
		/// </summary>
		public async Task<Dictionary<int, (int TotalAttendees, int AttendedCount)>> GetAttendanceStatsByCommitteeIdsAsync(List<int> committeeIds)
		{
			var stats = await ContextAsMMSContext.MeetingAttendees.AsNoTracking()
				.Where(x => x.Meeting.CommitteeId.HasValue && committeeIds.Contains(x.Meeting.CommitteeId.Value))
				.GroupBy(x => x.Meeting.CommitteeId!.Value)
				.Select(g => new
				{
					CommitteeId = g.Key,
					TotalAttendees = g.Count(),
					AttendedCount = g.Count(a => a.Attended == true)
				})
				.ToDictionaryAsync(x => x.CommitteeId, x => (x.TotalAttendees, x.AttendedCount));

			return stats;
		}
	}
}
