using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
	internal class VotingTypeOptionRepository : Repository<VotingOption>, IVotingTypeOptionRepository
	{
		public VotingTypeOptionRepository(DbContext context) : base(context)
		{
		}
	}
}
