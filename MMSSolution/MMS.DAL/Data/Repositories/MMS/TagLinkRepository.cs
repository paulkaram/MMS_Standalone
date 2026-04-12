using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using Task = System.Threading.Tasks.Task;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class TagLinkRepository : Repository<TagLink>, ITagLinkRepository
    {
        private MmsContext ContextAsMMSContext => (Context as MmsContext)!;

        public TagLinkRepository(DbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TagLink>> ListForEntityAsync(int entityTypeId, int entityId)
        {
            return await ContextAsMMSContext.Set<TagLink>()
                .Include(tl => tl.Tag)
                .Where(tl => tl.EntityTypeId == entityTypeId && tl.EntityId == entityId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> ListTagsForEntityAsync(int entityTypeId, int entityId)
        {
            return await ContextAsMMSContext.Set<TagLink>()
                .Where(tl => tl.EntityTypeId == entityTypeId && tl.EntityId == entityId)
                .Select(tl => tl.Tag)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<TagLink>> ListForEntitiesAsync(int entityTypeId, IEnumerable<int> entityIds)
        {
            var ids = entityIds.ToList();
            return await ContextAsMMSContext.Set<TagLink>()
                .Include(tl => tl.Tag)
                .Where(tl => tl.EntityTypeId == entityTypeId && ids.Contains(tl.EntityId))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task RemoveAllForEntityAsync(int entityTypeId, int entityId)
        {
            var existing = await ContextAsMMSContext.Set<TagLink>()
                .Where(tl => tl.EntityTypeId == entityTypeId && tl.EntityId == entityId)
                .ToListAsync();
            if (existing.Any())
            {
                ContextAsMMSContext.Set<TagLink>().RemoveRange(existing);
            }
        }
    }
}
