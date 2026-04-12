using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class CommitteeRoleRepository : Repository<CommitteeRole>, ICommitteeRoleRepository
    {
        public CommitteeRoleRepository(DbContext context) : base(context)
        {
        }
    }
}
