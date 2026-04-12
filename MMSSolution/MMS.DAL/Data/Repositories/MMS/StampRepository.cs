using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class StampRepository : Repository<Stamp>, IStampRepository
	{
		public StampRepository(DbContext context) : base(context)
		{
		}
	}
}
