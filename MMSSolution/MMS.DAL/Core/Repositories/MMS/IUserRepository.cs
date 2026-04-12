using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories.MMS
{
	public interface IUserRepository : IRepository<User>
	{
		Task<User?> GetIncludeCredentialsAndLanguageAndStructuresAsync(Expression<Func<User, bool>> filter);
		Task<string?> GetFullNameAsync(string userId, LanguageDbEnum language);
        string? GetFullName(string? userId, LanguageDbEnum language);
	}
}
