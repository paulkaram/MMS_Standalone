using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class DictionaryRepository : Repository<Dictionary>, IDictionaryRepository
	{
		public DictionaryRepository(DbContext context) : base(context)
		{
		}

	}
}