using MMS.DAL.Core.Repositories.Chats;
using MMS.DAL.Core.UnitOfWork.Chats;
using MMS.DAL.Data.Repositories.Chats;
using MMS.DAL.Models.Chat;

namespace MMS.DAL.Data.UnitOfWork.Chats
{
	internal class ChatUnitOfWork : UnitOfWork, IChatUnitOfWork
	{
		public IChatRepository Chats { get; private set; }

		public IChatMemberRepository ChatMembers { get; private set; }


		public IChatMessageRepository ChatMessages { get; private set; }


		public ChatUnitOfWork(InatalioChatContext context) : base(context)
		{
			Chats = new ChatRepository(context);
			ChatMembers = new ChatMemberRepository(context);
			ChatMessages = new ChatMessageRepository(context);
		
		}
	}
}
