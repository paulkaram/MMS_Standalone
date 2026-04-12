using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
	internal class MeetingAgendaRecommendationStatusRepository : Repository<MeetingAgendaRecommendationStatus>, IMeetingAgendaRecommendationStatusRepository
	{
		public MeetingAgendaRecommendationStatusRepository(DbContext context) : base(context)
		{
		}
	}
}
