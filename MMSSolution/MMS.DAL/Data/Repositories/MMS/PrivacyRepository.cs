using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
	internal class PrivacyRepository:Repository<Privacy>, IPrivaciesRepository
	{
		public PrivacyRepository(DbContext context) : base(context)
		{
		}
	}
}