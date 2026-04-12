using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.Chats;
using MMS.DAL.Models.Chat;

namespace MMS.DAL.Data.Repositories.Chats
{
	internal class ChatMessageRepository : Repository<ChatMessage>, IChatMessageRepository
	{
		public ChatMessageRepository(DbContext context) : base(context)
		{
		}
	}
}
