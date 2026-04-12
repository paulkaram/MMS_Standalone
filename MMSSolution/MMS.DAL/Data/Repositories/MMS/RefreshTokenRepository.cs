using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.Chat;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
	internal class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
	{
		MmsContext ContextAsMMSContext => (Context as MmsContext)!;

		public RefreshTokenRepository(DbContext context) : base(context)
		{
		}

		public async Task<bool> DeleteRefreshToken(string userId)
		{
		  await ContextAsMMSContext.RefreshTokens.Where(x => x.UserId == userId).ExecuteDeleteAsync();
			return true;
		}
	}
}
