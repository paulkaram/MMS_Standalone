using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class RoleTypeRepository : Repository<RoleType>, IRoleTypeRepository
	{
		public RoleTypeRepository(DbContext context) : base(context)
		{
		}

	}
}