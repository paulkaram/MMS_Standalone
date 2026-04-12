using MMS.DAL.Models.MMS;
using SysTask = System.Threading.Tasks.Task;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface ITagLinkRepository : IRepository<TagLink>
    {
        System.Threading.Tasks.Task<IEnumerable<TagLink>> ListForEntityAsync(int entityTypeId, int entityId);
        System.Threading.Tasks.Task<IEnumerable<Tag>> ListTagsForEntityAsync(int entityTypeId, int entityId);
        System.Threading.Tasks.Task<IEnumerable<TagLink>> ListForEntitiesAsync(int entityTypeId, IEnumerable<int> entityIds);
        SysTask RemoveAllForEntityAsync(int entityTypeId, int entityId);
    }
}
