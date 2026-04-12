using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories.MMS
{
	
	public interface IMeetingAgendaRecommendationsRepository : IRepository<MeetingAgendaRecommendation>
	{
		Task<List<MeetingAgendaRecommendation>> ListAsyncIncludeUserAndStatus(Expression<Func<MeetingAgendaRecommendation, bool>> filter);
		Task<List<MeetingAgendaRecommendation>> ListAsyncIncludeUserAndMeetingAndStatus(Expression<Func<MeetingAgendaRecommendation, bool>> filter, int page, int pageSize);
		Task<MeetingAgendaRecommendation> GetIncludeStatusAndUsers(Expression<Func<MeetingAgendaRecommendation, bool>> filter);
		Task<bool> ActivateRecommendationsStatus(int meetingId);
		Task<int?> GetCommitteeId(int recommendationId);
		Task<List<MeetingAgendaRecommendation>> ListByMeetingIdAsync(int meetingId);

		// Performance optimized batch methods
		Task<Dictionary<int, int>> GetCountsByCommitteeIdsAsync(List<int> committeeIds);
	}
}
