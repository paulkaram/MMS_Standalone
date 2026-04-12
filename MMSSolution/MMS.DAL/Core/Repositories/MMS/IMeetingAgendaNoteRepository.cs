using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories.MMS
{
	public interface IMeetingAgendaNoteRepository : IRepository<MeetingAgendaNote>
	{
		Task<List<MeetingAgendaNote>> ListAsyncIncludeUser(Expression<Func<MeetingAgendaNote, bool>> filter);
		Task<MeetingAgendaNote?> GetAsyncIncludeAgenda(Expression<Func<MeetingAgendaNote, bool>> filter);
	}
}
