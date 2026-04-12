using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class PermissionLevelRepository : Repository<PermissionLevel>, IPermissionLevelRepository
	{
		public PermissionLevelRepository(DbContext context) : base(context)
		{
		}

	}
}