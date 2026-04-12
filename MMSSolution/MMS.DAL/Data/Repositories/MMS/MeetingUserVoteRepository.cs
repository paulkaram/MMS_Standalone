using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;
namespace MMS.DAL.Data.Repositories.MMS
{
	
	internal class MeetingUserVoteRepository : Repository<MeetingUserVote>, IMeetingUserVoteRepository
	{
		MmsContext ContextAsMMSContext => (Context as MmsContext)!;
		public MeetingUserVoteRepository(DbContext context) : base(context)
		{

		}
		public async Task<List<MeetingUserVote>> ListIncludeMeetingAgendaAndUserAndVottingOptionAsync(Expression<Func<MeetingUserVote, bool>> filter)
		{
			return await ContextAsMMSContext.MeetingUserVotes
				.Include(c => c.MeetingAgenda)
				.Include(c => c.User)
				.Include(c => c.VottingOption)
				.Where(filter)
				.ToListAsync();
		}
		public async Task<List<MeetingUserVote>> ListIncludeMeetingAgendaAndVotingOptionAsync(Expression<Func<MeetingUserVote, bool>> filter)
		{
			return await ContextAsMMSContext.MeetingUserVotes
				.Include(c => c.MeetingAgenda)
				.Include(c => c.VottingOption)
				.Where(filter)
				.ToListAsync();
		}

	}
}
