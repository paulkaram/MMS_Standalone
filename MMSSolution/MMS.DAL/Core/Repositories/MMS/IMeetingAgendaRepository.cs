using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface IMeetingAgendaRepository : IRepository<MeetingAgendum>
    {
		Task<List<MeetingAgendum>?> ListIncludeVotingAndDutyAndTopicsAsync(Expression<Func<MeetingAgendum, bool>> filter);
		Task<List<MeetingAgendum>?> ListIncludeVotingAndDutyAndTopicsAsyncWithTrack(Expression<Func<MeetingAgendum, bool>> filter);
		Task<List<MeetingAgendum>?> ListIncludeTopicsAndVotingTypeAsync(Expression<Func<MeetingAgendum, bool>> filter);
		Task<string?> GetMeetingOwner(int id);
		Task<MeetingAgendum?> GetIncludeSummaryAsync(Expression<Func<MeetingAgendum, bool>> filter);
		Task<List<MeetingAgendum>?> ListIncludeAllForMinutesAsync(Expression<Func<MeetingAgendum, bool>> filter);
	}
}
