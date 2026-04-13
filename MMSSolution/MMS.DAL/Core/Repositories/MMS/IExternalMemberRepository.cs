using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
    public interface IExternalMemberRepository : IRepository<ExternalMember>
    {
        System.Threading.Tasks.Task<ExternalMember?> GetByEmailAsync(string email);
    }
}
