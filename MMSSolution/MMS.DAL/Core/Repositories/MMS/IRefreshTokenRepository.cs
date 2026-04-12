using MMS.DAL.Models.MMS;

namespace MMS.DAL.Core.Repositories.MMS
{
	public interface IRefreshTokenRepository : IRepository<RefreshToken>
	{
		Task<bool> DeleteRefreshToken(string userId);
	}
}
