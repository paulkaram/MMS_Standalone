using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class GroupRepository : Repository<Group>, IGroupRepository
    {
        public GroupRepository(DbContext context) : base(context) { }
    }
}
