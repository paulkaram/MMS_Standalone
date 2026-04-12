using MMS.DAL.Models.Chat;
using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories.Chats
{
	public interface IChatRepository : IRepository<Chat>
	{
		Task<bool> CheckPrivateChatExist(List<string> usersIds);
		Task<Chat> FindAsync(int id);
		Task<List<Chat>> ListIncludeChatMembersAsync<TOrderKey>(Expression<Func<Chat, bool>> filter, Expression<Func<Chat, TOrderKey>>? orderBy= null,bool descending = true);
		Task UpdateLastChange(int chatId, DateTime sentAt);
	}
}
