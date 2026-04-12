using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
namespace MMS.DAL.Core.Repositories.MMS
{
    public interface IStructureRepository : IRepository<Structure>
    {
        public Task<List<Structure>?> ListIncludeUserAsync(System.Linq.Expressions.Expression<Func<Structure, bool>> filter);
        public Task<Structure?> GetIncludeUserAsync(System.Linq.Expressions.Expression<Func<Structure, bool>> filter);
        public Task<Structure?> GetIncludeUserAndRoleAsync(System.Linq.Expressions.Expression<Func<Structure, bool>> filter);
        string? GetFullName(string? structureId, LanguageDbEnum language);
        List<Role>? ListRolesInStructure(int structureId);
    }
}