using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class ExternalMemberRepository : Repository<ExternalMember>, IExternalMemberRepository
    {
        private MmsContext ContextAsMMSContext => (Context as MmsContext)!;

        public ExternalMemberRepository(DbContext context) : base(context)
        {
        }

        public async System.Threading.Tasks.Task<ExternalMember?> GetByEmailAsync(string email)
        {
            return await ContextAsMMSContext.Set<ExternalMember>()
                .FirstOrDefaultAsync(e => e.Email == email);
        }
    }
}
