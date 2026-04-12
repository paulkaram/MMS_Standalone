using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class LanguageRepository : Repository<Language>, ILanguageRepository
	{
		public LanguageRepository(DbContext context) : base(context)
		{
		}

	}
}