using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.Chats;
using MMS.DAL.Models.Chat;

namespace MMS.DAL.Data.Repositories.Chats
{
	internal class ChatMemberRepository : Repository<ChatMember>, IChatMemberRepository
	{
		public ChatMemberRepository(DbContext context) : base(context)
		{
		}
	}
}
