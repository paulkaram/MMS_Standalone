using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
	internal class UserGroupRepository : Repository<UserGroup>, IUserGroupRepository
	{
		public UserGroupRepository(DbContext context) : base(context)
		{
		}
	}
}
