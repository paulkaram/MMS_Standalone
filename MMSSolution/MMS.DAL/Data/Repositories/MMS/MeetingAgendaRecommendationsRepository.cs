using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;


namespace MMS.DAL.Data.Repositories.MMS
{
	internal class MeetingAgendaRecommendationsRepository : Repository<MeetingAgendaRecommendation>, IMeetingAgendaRecommendationsRepository
	{
		MmsContext ContextAsMMSContext => (Context as MmsContext)!;
		public MeetingAgendaRecommendationsRepository(DbContext context) : base(context)
		{
		}

		public async Task<List<MeetingAgendaRecommendation>> ListAsyncIncludeUserAndStatus(Expression<Func<MeetingAgendaRecommendation, bool>> filter)
		{
			return await ContextAsMMSContext.MeetingAgendaRecommendations.
				Include(x => x.OwnerNavigation).
				Include(x => x.CreateByNavigation).
				Include(x => x.Status).
				Include(x => x.Priority).
				Where(filter).ToListAsync();
		}
		public async Task<List<MeetingAgendaRecommendation>> ListAsyncIncludeUserAndMeetingAndStatus(Expression<Func<MeetingAgendaRecommendation, bool>> filter, int page, int pageSize)
		{
			return await ContextAsMMSContext.MeetingAgendaRecommendations.
				Include(x => x.OwnerNavigation).
				Include(x => x.CreateByNavigation).
				Include(x => x.Status).
				Include(x => x.MeetingAgenda)
				.ThenInclude(x => x.Meeting)
				.OrderByDescending(x => x.Id)
				.Where(filter).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

		}
		public async Task<MeetingAgendaRecommendation> GetIncludeStatusAndUsers(Expression<Func<MeetingAgendaRecommendation, bool>> filter)
		{
			return await ContextAsMMSContext.MeetingAgendaRecommendations.
				Include(x => x.OwnerNavigation).
				Include(x => x.CreateByNavigation).
				Include(x => x.Status).
				Include(x => x.Priority)
				.FirstOrDefaultAsync(filter);

		}
		public async Task<bool> ActivateRecommendationsStatus(int meetingId)
		{
			//bulk update to enhance the performance and directly update the records without loading them
			return await ContextAsMMSContext.MeetingAgendaRecommendations.
				Where(x => x.MeetingAgenda.MeetingId == meetingId).
				ExecuteUpdateAsync(x =>
				x.SetProperty(x => x.StatusId, (int)MeetingAgendaRecommendationStatusDbEnum.InProgess)) > 0;
		}

		public async Task<int?> GetCommitteeId(int id)
		{
			return await ContextAsMMSContext.MeetingAgendaRecommendations.Where(x => x.Id == id)
			.Select(x => x.MeetingAgenda.Meeting.CommitteeId)
			.FirstOrDefaultAsync();
		}

		public async Task<List<MeetingAgendaRecommendation>> ListByMeetingIdAsync(int meetingId)
		{
			return await ContextAsMMSContext.MeetingAgendaRecommendations
				.Include(x => x.MeetingAgenda)
				.Where(x => x.MeetingAgenda.MeetingId == meetingId &&
					x.StatusId != (int)MeetingAgendaRecommendationStatusDbEnum.Draft)
				.ToListAsync();
		}

		// Performance optimized batch methods

		/// <summary>
		/// Get recommendation counts grouped by committee IDs in a single query
		/// </summary>
		public async Task<Dictionary<int, int>> GetCountsByCommitteeIdsAsync(List<int> committeeIds)
		{
			return await ContextAsMMSContext.MeetingAgendaRecommendations.AsNoTracking()
				.Where(x => x.MeetingAgenda.Meeting.CommitteeId.HasValue &&
					committeeIds.Contains(x.MeetingAgenda.Meeting.CommitteeId.Value))
				.GroupBy(x => x.MeetingAgenda.Meeting.CommitteeId!.Value)
				.Select(g => new { CommitteeId = g.Key, Count = g.Count() })
				.ToDictionaryAsync(x => x.CommitteeId, x => x.Count);
		}
	}
}
