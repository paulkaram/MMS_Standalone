using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class TagRepository : Repository<Tag>, ITagRepository
    {
        private MmsContext ContextAsMMSContext => (Context as MmsContext)!;

        public TagRepository(DbContext context) : base(context)
        {
        }

        public async Task<Tag?> GetByNameAsync(string nameEn)
        {
            return await ContextAsMMSContext.Set<Tag>()
                .FirstOrDefaultAsync(t => t.NameEn == nameEn);
        }
    }
}
