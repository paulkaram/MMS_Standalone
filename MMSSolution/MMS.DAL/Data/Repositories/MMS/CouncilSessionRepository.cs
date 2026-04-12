using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{

	internal class CouncilSessionRepository : Repository<CouncilSession>, ICouncilSessionRepository
	{
		public CouncilSessionRepository(DbContext context) : base(context)
		{
		}
	}
}
