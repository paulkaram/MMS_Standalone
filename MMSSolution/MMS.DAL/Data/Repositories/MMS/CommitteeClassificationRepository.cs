using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class CommitteeClassificationRepository : Repository<CommitteeClassification>, ICommitteeClassificationRepository
    {
        public CommitteeClassificationRepository(DbContext context) : base(context)
        {
        }
    }
}
