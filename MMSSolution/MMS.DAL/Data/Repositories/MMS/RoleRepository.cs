using MMS.DAL.Enumerations;
using Microsoft.EntityFrameworkCore;
using MMS.DAL.Models.MMS;
using MMS.DAL.Core.Repositories.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class RoleRepository : Repository<Role>, IRoleRepository
	{
        MmsContext ContextAsMMSContext => (Context as MmsContext)!;

        public RoleRepository(DbContext context) : base(context)
		{
		}
		public async Task<string?> GetFullNameAsync(string roleId, LanguageDbEnum language)
		{
			return await ContextAsMMSContext.Roles.Where(x => x.Id == Convert.ToInt32(roleId)).Select(x => language == LanguageDbEnum.Arabic ? x.RoleNameAr : x.RoleNameEn)
				.FirstOrDefaultAsync();
		}

		public string? GetFullName(string roleId, LanguageDbEnum language)
		{
			return ContextAsMMSContext.Roles.Where(x => x.Id == Convert.ToInt32(roleId)).Select(x => language == LanguageDbEnum.Arabic ? x.RoleNameAr : x.RoleNameEn)
				.FirstOrDefault();
		}
	}
}