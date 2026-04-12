using MMS.DAL.Core.Repositories.Chats;


namespace MMS.DAL.Core.UnitOfWork.Chats
{
	public interface  IChatUnitOfWork : IUnitOfWork
	{
		IChatRepository Chats { get; }
		IChatMemberRepository ChatMembers { get; }
		IChatMessageRepository ChatMessages { get; }
	}
	
}
