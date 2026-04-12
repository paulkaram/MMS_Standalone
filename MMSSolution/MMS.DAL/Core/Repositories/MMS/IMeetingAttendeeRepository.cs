using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories.MMS
{
	public interface IMeetingAttendeeRepository : IRepository<MeetingAttendee>
	{
		Task<List<MeetingAttendee>> ListIncludeUserAsync(Expression<Func<MeetingAttendee, bool>> filter);
		Task<List<MeetingAttendee>> ListIncludeUserAndMeetingAndComitteeAsync(Expression<Func<MeetingAttendee, bool>> filter,int page, int pageSize);

		// Performance optimized batch methods
		Task<Dictionary<int, (int TotalAttendees, int AttendedCount)>> GetAttendanceStatsByCommitteeIdsAsync(List<int> committeeIds);
	}
}
