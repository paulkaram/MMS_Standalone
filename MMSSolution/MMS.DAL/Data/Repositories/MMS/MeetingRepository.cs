using Azure;
using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class MeetingRepository : Repository<Meeting>, IMeetingRepository
    {
        MmsContext ContextAsMMSContext => (Context as MmsContext)!;
        public MeetingRepository(DbContext context) : base(context)
        {

        }

        public async Task<List<Meeting>?> ListIncludeCommitteeAndStatusAsync(Expression<Func<Meeting, bool>> filter, int page, int pageSize)
        {
            return await ContextAsMMSContext.Meetings.Where(filter)
                .Include(x => x.Committee)
                .Include(x => x.Status)
                .Include(x => x.CreatedbyNavigation)
                .Include(x => x.Tasks)
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<Meeting?> GetIncludeAttendeesAsync(Expression<Func<Meeting, bool>> filter)
        {
            return await ContextAsMMSContext.Meetings
                .Include(x => x.MeetingAttendees).ThenInclude(x => x.User).FirstOrDefaultAsync(filter);
        }

        public async Task<Meeting?> GetIncludeAssociatedAsync(Expression<Func<Meeting, bool>> filter)
        {
            return await ContextAsMMSContext.Meetings
                .Include(x => x.Associateds).FirstOrDefaultAsync(filter);
        }

        public async Task<Meeting?> GetIncludeAttendeesAndAssociatedAsync(Expression<Func<Meeting, bool>> filter)
        {
            return await ContextAsMMSContext.Meetings.AsNoTracking()
                .Include(x => x.MeetingAttendees)
                .ThenInclude(m => m.User)
                .Include(x => x.Committee)
                .Include(x => x.CouncilSession)
                .Include(x => x.Associateds)
                .Include(x => x.Status)
                .FirstOrDefaultAsync(filter);
        }
        public async Task<Meeting?> GetIncludeAttendeesAgendasRecommendationsAsync(Expression<Func<Meeting, bool>> filter)
        {
            return await ContextAsMMSContext.Meetings.AsNoTracking()
                .Include(x => x.MeetingAttendees)
                .ThenInclude(m => m.User)
                .Include(x => x.Committee)
                .Include(x => x.MeetingAgenda)
                .ThenInclude(x => x.MeetingAgendaRecommendations)
                .FirstOrDefaultAsync(filter);
        }

        public async Task<Meeting?> GetIncludeSummaryAsync(Expression<Func<Meeting, bool>> filter)
        {
            return await ContextAsMMSContext.Meetings
                .Include(x => x.MeetingSummary)
                .FirstOrDefaultAsync(filter);
        }
        public async Task<List<Meeting>?> ListIncludeCommitteeAndUserAndAgendaAndAgendaTopicAndAgendaNotesAndRecommendationAsync(Expression<Func<Meeting, bool>> filter)
        {
            return await ContextAsMMSContext.Meetings.AsNoTracking().Where(filter)
                .Include(x => x.Committee)
                .ThenInclude(x => x.UserCommittees)
                .ThenInclude(x => x.User)
                .Include(x => x.MeetingAgenda)
                .ThenInclude(x => x.AgendaTopics)
                .Include(x => x.MeetingAgenda)
                .ThenInclude(x => x.MeetingAgendaNotes)
                .Include(x => x.MeetingAgenda)
                .ThenInclude(x => x.MeetingAgendaRecommendations).OrderByDescending(x => x.Id)
                .ToListAsync();
        }
        public async Task<Meeting?> GetIncludeUsersAsync(Expression<Func<Meeting, bool>> filter)
        {
            return await ContextAsMMSContext.Meetings.AsNoTracking()
                .Include(x => x.MeetingAttendees)
                .ThenInclude(y => y.User)
                .Include(x => x.CreatedbyNavigation)
                .FirstOrDefaultAsync(filter);
        }

        // Performance optimized batch methods

        /// <summary>
        /// Get meeting counts grouped by committee IDs in a single query
        /// </summary>
        public async Task<Dictionary<int, int>> GetCountsByCommitteeIdsAsync(List<int> committeeIds, int? excludeStatus = null)
        {
            var query = ContextAsMMSContext.Meetings.AsNoTracking()
                .Where(x => committeeIds.Contains(x.CommitteeId ?? 0));

            if (excludeStatus.HasValue)
            {
                query = query.Where(x => x.StatusId != excludeStatus.Value);
            }

            return await query
                .GroupBy(x => x.CommitteeId ?? 0)
                .Select(g => new { CommitteeId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CommitteeId, x => x.Count);
        }

        /// <summary>
        /// Get meeting counts grouped by status for a specific committee in a single query
        /// </summary>
        public async Task<Dictionary<int, int>> GetCountsByStatusForCommitteeAsync(int committeeId, List<int> statusIds)
        {
            return await ContextAsMMSContext.Meetings.AsNoTracking()
                .Where(x => x.CommitteeId == committeeId && x.StatusId.HasValue && statusIds.Contains(x.StatusId.Value))
                .GroupBy(x => x.StatusId!.Value)
                .Select(g => new { StatusId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StatusId, x => x.Count);
        }

        /// <summary>
        /// Get meeting counts grouped by status for a specific user in a single query
        /// </summary>
        public async Task<Dictionary<int, int>> GetCountsByStatusForUserAsync(string userId, List<int> statusIds)
        {
            return await ContextAsMMSContext.Meetings.AsNoTracking()
                .Where(x => (x.Createdby == userId || x.MeetingAttendees.Any(a => a.UserId == userId))
                         && x.StatusId.HasValue && statusIds.Contains(x.StatusId.Value))
                .GroupBy(x => x.StatusId!.Value)
                .Select(g => new { StatusId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StatusId, x => x.Count);
        }

        /// <summary>
        /// Get upcoming meetings with projection applied at database level
        /// </summary>
        public async Task<List<T>> GetUpcomingMeetingsAsync<T>(string userId, DateTime fromDate, int count, Expression<Func<Meeting, T>> selector)
        {
            return await ContextAsMMSContext.Meetings.AsNoTracking()
                .Include(x => x.Committee)
                .Include(x => x.Status)
                .Where(x => (x.Createdby == userId || x.MeetingAttendees.Any(a => a.UserId == userId))
                         && x.Date >= fromDate
                         && x.StatusId != (int)Enumerations.MeetingStatusDbEnum.Canceled)
                .OrderBy(x => x.Date)
                .ThenBy(x => x.StartTime)
                .Take(count)
                .Select(selector)
                .ToListAsync();
        }

        /// <summary>
        /// Get recent meeting activities with projection applied at database level
        /// </summary>
        public async Task<List<T>> GetRecentActivitiesAsync<T>(string userId, int count, Expression<Func<Meeting, T>> selector)
        {
            return await ContextAsMMSContext.Meetings.AsNoTracking()
                .Where(x => x.Createdby == userId || x.MeetingAttendees.Any(a => a.UserId == userId))
                .OrderByDescending(x => x.CreatedDate)
                .Take(count)
                .Select(selector)
                .ToListAsync();
        }
    }
}
