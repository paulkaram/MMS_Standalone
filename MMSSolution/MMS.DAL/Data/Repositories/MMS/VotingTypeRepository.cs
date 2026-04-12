using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class VotingTypeRepository : Repository<VotingType>, IVotingTypeRepository
    {
        public VotingTypeRepository(DbContext context) : base(context)
        {
        }
    }
}
