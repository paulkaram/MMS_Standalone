using MMS.DAL.Models.MMS;
using System.Linq.Expressions;
namespace MMS.DAL.Core.Repositories.MMS
{
	public interface IMeetingUserVoteRepository : IRepository<MeetingUserVote>
	{
		Task<List<MeetingUserVote>> ListIncludeMeetingAgendaAndUserAndVottingOptionAsync(Expression<Func<MeetingUserVote, bool>> filter);
		Task<List<MeetingUserVote>> ListIncludeMeetingAgendaAndVotingOptionAsync(Expression<Func<MeetingUserVote, bool>> filter);
	}
}
