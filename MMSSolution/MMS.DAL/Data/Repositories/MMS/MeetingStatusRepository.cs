using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
	internal class MeetingStatusRepository : Repository<MeetingStatus>, IMeetingStatusRepository
	{
		public MeetingStatusRepository(DbContext context) : base(context)
		{
		}
	}
}
