using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface IMeetingRepository : IRepository<Meeting>
    {
        Task<List<Meeting>?> ListIncludeCommitteeAndStatusAsync(Expression<Func<Meeting, bool>> filter, int page,int pageSize);
        Task<Meeting?> GetIncludeAttendeesAsync(Expression<Func<Meeting, bool>> filter);
        Task<Meeting?> GetIncludeAssociatedAsync(Expression<Func<Meeting, bool>> filter);
        Task<Meeting?> GetIncludeAttendeesAndAssociatedAsync(Expression<Func<Meeting, bool>> filter);
        Task<Meeting?> GetIncludeAttendeesAgendasRecommendationsAsync(Expression<Func<Meeting, bool>> filter);
		Task<Meeting?> GetIncludeSummaryAsync(Expression<Func<Meeting, bool>> filter);

		Task<List<Meeting>?> ListIncludeCommitteeAndUserAndAgendaAndAgendaTopicAndAgendaNotesAndRecommendationAsync(Expression<Func<Meeting, bool>> filter);
		Task<Meeting?> GetIncludeUsersAsync(Expression<Func<Meeting, bool>> filter);

		// Performance optimized batch methods
		Task<Dictionary<int, int>> GetCountsByCommitteeIdsAsync(List<int> committeeIds, int? excludeStatus = null);
		Task<Dictionary<int, int>> GetCountsByStatusForCommitteeAsync(int committeeId, List<int> statusIds);
		Task<Dictionary<int, int>> GetCountsByStatusForUserAsync(string userId, List<int> statusIds);
		Task<List<T>> GetUpcomingMeetingsAsync<T>(string userId, DateTime fromDate, int count, Expression<Func<Meeting, T>> selector);
		Task<List<T>> GetRecentActivitiesAsync<T>(string userId, int count, Expression<Func<Meeting, T>> selector);
	}
}
