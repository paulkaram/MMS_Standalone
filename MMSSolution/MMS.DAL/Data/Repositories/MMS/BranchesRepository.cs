using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
	internal class BranchesRepository : Repository<Branch>, IBranchesRepository
	{
		public BranchesRepository(DbContext context) : base(context)
		{
		}

	}
}
