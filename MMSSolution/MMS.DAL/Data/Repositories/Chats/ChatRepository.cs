using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.Chats;
using MMS.DAL.Models.Chat;
using System;
using System.Linq.Expressions;


namespace MMS.DAL.Data.Repositories.Chats
{
	internal class ChatRepository : Repository<Chat>, IChatRepository
	{
		InatalioChatContext ContextAsChatContext => (Context as InatalioChatContext)!;

		public ChatRepository(DbContext context) : base(context)
		{
		}

		public async Task<List<Chat>> ListIncludeChatMembersAsync<TOrderKey>(Expression<Func<Chat, bool>> filter, Expression<Func<Chat, TOrderKey>>? orderBy = null, bool descending = true)
		{
			var query = ContextAsChatContext.Chats.Include(x => x.ChatMembers).Include(x=>x.ChatMessages).AsQueryable();

			if (filter != null)
			{
				query = query.Where(filter);
			}

			if (orderBy != null)
			{
				query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
			}

			return await query.ToListAsync();
		}

		public async Task<bool> CheckPrivateChatExist(List<string> usersIds)
		{
			var userIdsCount = usersIds.Count;
			return await ContextAsChatContext.Chats
				.AnyAsync(c => !c.IsGroup &&
					c.ChatMembers.Count == userIdsCount && // Ensure the chat has exactly the same number of members
					c.ChatMembers.All(m => usersIds.Contains(m.UserId))); // All members must be in usersIds

			
		}

		public async Task<Chat> FindAsync(int id)
		{
			return await ContextAsChatContext.Chats.Include(x => x.ChatMembers).FirstOrDefaultAsync(x => x.Id == id);
		}

		public async Task UpdateLastChange(int chatId, DateTime sentAt)
		{
			var chat = await ContextAsChatContext.Chats.FindAsync(chatId);
			chat.UpdatedAt = sentAt;
		}
	}
}
