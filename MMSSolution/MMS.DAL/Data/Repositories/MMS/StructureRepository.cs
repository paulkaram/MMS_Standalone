using MMS.DAL.Enumerations;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using MMS.DAL.Models.MMS;
using MMS.DAL.Core.Repositories.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class StructureRepository : Repository<Structure>, IStructureRepository
    {
        MmsContext ContextAsMMSContext => (Context as MmsContext)!;
        public StructureRepository(DbContext context) : base(context)
        {
        }

        public async Task<List<Structure>?> ListIncludeUserAsync(Expression<Func<Structure, bool>> filter)
        {
            return await ContextAsMMSContext.Structures
             .Include(x => x.UserStructures)
             .ThenInclude(x => x.User).ToListAsync();
        }

        public async Task<Structure?> GetIncludeUserAsync(Expression<Func<Structure, bool>> filter)
        {
            return await ContextAsMMSContext.Structures
             .Include(x => x.UserStructures)
             .ThenInclude(x => x.User).FirstOrDefaultAsync(filter);
        }

        public async Task<Structure?> GetIncludeUserAndRoleAsync(Expression<Func<Structure, bool>> filter)
        {
            return await ContextAsMMSContext.Structures
             .Include(x => x.UserStructures)
             .ThenInclude(x => x.User)
             .Include(x => x.UserStructures)
             .ThenInclude(x => x.Role).FirstOrDefaultAsync(filter);
        }

        public List<Role>? ListRolesInStructure(int structureId)
        {
            return ContextAsMMSContext.Structures.Include(x => x.UserStructures).ThenInclude(x => x.Role).FirstOrDefault(x => x.Id == structureId)?.UserStructures.Select(x => x.Role).ToList();
        }

        public string? GetFullName(string? structureId, LanguageDbEnum language)
        {
            return ContextAsMMSContext.Structures.Where(x => x.Id == Convert.ToInt32(structureId)).Select(x => language == LanguageDbEnum.Arabic ? x.NameAr : x.NameEn)
            .FirstOrDefault();
        }
    }
}