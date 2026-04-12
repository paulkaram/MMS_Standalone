using Microsoft.EntityFrameworkCore;
using MMS.DAL.Models.MMS;
using MMS.DAL.Core.Repositories.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class AppSettingRepository : Repository<AppSetting>, IAppSettingRepository
	{
		public AppSettingRepository(DbContext context) : base(context)
		{
		}

	}
}