using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface ICommitteeExternalMemberRepository : IRepository<CommitteeExternalMember>
    {
        System.Threading.Tasks.Task<IEnumerable<CommitteeExternalMember>> ListByCommitteeAsync(int committeeId);
    }
}
