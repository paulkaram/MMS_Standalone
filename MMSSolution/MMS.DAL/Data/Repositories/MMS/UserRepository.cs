using MMS.DAL.Enumerations;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using MMS.DAL.Models.MMS;
using MMS.DAL.Core.Repositories.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class UserRepository : Repository<User>, IUserRepository
	{
        MmsContext ContextAsMMSContext => (Context as MmsContext)!;

        public UserRepository(DbContext context) : base(context)
		{
		}

		public async Task<User?> GetIncludeCredentialsAndLanguageAndStructuresAsync(Expression<Func<User, bool>> filter)
		{
			return await ContextAsMMSContext.Users
				.Include(x => x.DefaultLanguage)
				.Include(x => x.UserStructures)
				.FirstOrDefaultAsync(filter);
		}

		public async Task<string?> GetFullNameAsync(string userId, LanguageDbEnum language)
		{
			return await ContextAsMMSContext.Users.Where(x => x.Id == userId).Select(x => language == LanguageDbEnum.Arabic ? x.FullnameAr : x.FullnameEn)
				.FirstOrDefaultAsync();
		}

		public string? GetFullName(string userId, LanguageDbEnum language)
		{
			return ContextAsMMSContext.Users.Where(x => x.Id == userId).Select(x => language == LanguageDbEnum.Arabic ? x.FullnameAr : x.FullnameEn)
					.FirstOrDefault();
		}

    }
}
