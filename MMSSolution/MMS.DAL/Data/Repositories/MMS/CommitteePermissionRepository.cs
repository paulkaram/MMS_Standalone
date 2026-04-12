using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories.MMS
{
	internal class CommitteePermissionRepository : Repository<CommitteePermission>, ICommitteePermissionRepository
	{
		MmsContext ContextAsMMSContext => (Context as MmsContext)!;
		public CommitteePermissionRepository(DbContext context) : base(context)
		{
		}

		public async Task<List<CommitteePermission>> ListIncludeCommitteeAsync(Expression<Func<CommitteePermission, bool>> filter)
		{
			return await ContextAsMMSContext.CommitteePermissions.Include(x => x.Committee)
                .Where(x => x.Committee.CommitteeStatusId == (int)CommitteeStatusDbEnum.Active)
                .Where(filter).ToListAsync();
		}
	}
}
