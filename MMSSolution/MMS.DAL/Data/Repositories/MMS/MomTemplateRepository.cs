using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class MomTemplateRepository : Repository<MomTemplate>, IMomTemplateRepository
    {
        public MomTemplateRepository(DbContext context) : base(context)
        {
        }
    }
}
