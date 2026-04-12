
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
	public interface IRoleRepository : IRepository<Role>
	{
		Task<string?> GetFullNameAsync(string roleId, LanguageDbEnum language);
        new string? GetFullName(string? roleId, LanguageDbEnum language);
	}
}