using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class MeetingAgendaRepository : Repository<MeetingAgendum>, IMeetingAgendaRepository
    {
        MmsContext ContextAsMMSContext => (Context as MmsContext)!;
        public MeetingAgendaRepository(DbContext context) : base(context)
        {
        }
        public async Task<List<MeetingAgendum>?> ListIncludeVotingAndDutyAndTopicsAsync(Expression<Func<MeetingAgendum, bool>> filter)
        {
            return await ContextAsMMSContext.MeetingAgenda.AsNoTracking()
                .Include(x => x.VotingType)
                .ThenInclude(v=>v.VotingOptions)
                .Include(x => x.AgendaTopics)
                .Include(x=>x.MeetingUserVotes)
                    .ThenInclude(v => v.User)
                .Include(x=>x.MeetingUserVotes)
                    .ThenInclude(v => v.VottingOption)
                 .Where(filter).ToListAsync();
        }
		public async Task<List<MeetingAgendum>?> ListIncludeVotingAndDutyAndTopicsAsyncWithTrack(Expression<Func<MeetingAgendum, bool>> filter)
		{
			return await ContextAsMMSContext.MeetingAgenda
				.Include(x => x.VotingType)
				.ThenInclude(v => v.VotingOptions)
				.Include(x => x.AgendaTopics)
				.Include(x => x.MeetingUserVotes)
					.ThenInclude(v => v.User)
				.Include(x => x.MeetingUserVotes)
					.ThenInclude(v => v.VottingOption)
				 .Where(filter).ToListAsync();
		}
		public async Task<List<MeetingAgendum>?> ListIncludeTopicsAndVotingTypeAsync(Expression<Func<MeetingAgendum, bool>> filter)
		{
			return await ContextAsMMSContext.MeetingAgenda.AsNoTracking()
				.Include(x => x.VotingType)
				.Include(x => x.AgendaTopics)
				 .Where(filter).ToListAsync();
		}
		public async Task<string?> GetMeetingOwner(int id)
        {
            var agenda = await ContextAsMMSContext.MeetingAgenda.Include(x => x.Meeting).Where(x => x.Id == id).FirstOrDefaultAsync();
            if (agenda!=null)
            {
				return agenda.Meeting.Createdby;
			}
            return null;
        }

		public async Task<MeetingAgendum?> GetIncludeSummaryAsync(Expression<Func<MeetingAgendum, bool>> filter)
		{
			return await ContextAsMMSContext.MeetingAgenda
				.Include(x => x.MeetingAgendaSummaries)
				.FirstOrDefaultAsync(filter);
		}

		/// <summary>
		/// Gets agendas with all related data needed for minutes generation:
		/// - Voting type and options
		/// - User votes with voter details
		/// - Notes with author details
		/// - Recommendations with owner details
		/// - Agenda summaries
		/// </summary>
		public async Task<List<MeetingAgendum>?> ListIncludeAllForMinutesAsync(Expression<Func<MeetingAgendum, bool>> filter)
		{
			return await ContextAsMMSContext.MeetingAgenda.AsNoTracking()
				.Include(x => x.VotingType)
					.ThenInclude(v => v.VotingOptions)
				.Include(x => x.MeetingUserVotes)
					.ThenInclude(v => v.User)
				.Include(x => x.MeetingUserVotes)
					.ThenInclude(v => v.VottingOption)
				.Include(x => x.MeetingAgendaNotes)
					.ThenInclude(n => n.User)
				.Include(x => x.MeetingAgendaRecommendations)
					.ThenInclude(r => r.OwnerNavigation)
				.Include(x => x.MeetingAgendaSummaries)
				.Where(filter)
				.OrderBy(x => x.Id)
				.ToListAsync();
		}
    }
}
