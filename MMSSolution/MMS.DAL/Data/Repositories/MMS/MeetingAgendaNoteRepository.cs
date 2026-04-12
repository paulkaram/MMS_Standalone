using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories.MMS
{
	internal class MeetingAgendaNoteRepository : Repository<MeetingAgendaNote>, IMeetingAgendaNoteRepository
	{
		MmsContext ContextAsMMSContext => (Context as MmsContext)!;
		public MeetingAgendaNoteRepository(DbContext context) : base(context)
		{
		}

		public async Task<List<MeetingAgendaNote>> ListAsyncIncludeUser(Expression<Func<MeetingAgendaNote, bool>> filter)
		{
			return await ContextAsMMSContext.MeetingAgendaNotes.Include(x=>x.User).Where(filter).ToListAsync();
		}

		public async Task<MeetingAgendaNote?> GetAsyncIncludeAgenda(Expression<Func<MeetingAgendaNote, bool>> filter)
		{
			return await ContextAsMMSContext.MeetingAgendaNotes.Include(x => x.MeetingAgenda).FirstOrDefaultAsync(filter);
		}
	}
}
