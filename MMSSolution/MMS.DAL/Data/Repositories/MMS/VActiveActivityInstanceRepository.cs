using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class VActiveActivityInstanceRepository : Repository<VActiveActivityInstance>, IVActiveActivityInstanceRepository
	{
		public VActiveActivityInstanceRepository(DbContext context) : base(context)
		{
		}

	}
}