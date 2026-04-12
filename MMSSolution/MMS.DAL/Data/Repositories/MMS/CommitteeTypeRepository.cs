using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class CommitteeTypeRepository : Repository<CommitteeType>, ICommitteeTypeRepository
    {
        public CommitteeTypeRepository(DbContext context) : base(context)
        {
        }
    }
}
