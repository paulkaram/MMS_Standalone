using Mapster;
using MMS.DAL.Models.Chat;
using MMS.DTO.Chats;

namespace MMS.BLL.Mapping
{
	internal class ChatMappingConfiguration : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{

			config.NewConfig<string, ChatMember>()
			.Map(dest => dest.UserId, src => src);
			config.NewConfig<ChatPostDto, Chat>()
				.Map(dest => dest.ChatMembers, src => src.UsersIds);
			config.NewConfig<ChatMessagePostDto, ChatMessage>()
				.Map(dest => dest.SentAt, src => DateTime.Now);
			config.NewConfig<(Chat chat,string UserId), ChatListDto>()
				.Map(dest => dest.ChatMembers, src => src.chat.ChatMembers)
				.Map(dest => dest.Id, src => src.chat.Id)
				.Map(dest => dest.UpdatedAt, src => src.chat.UpdatedAt)
				.Map(dest => dest.IsGroup, src => src.chat.IsGroup)
				.Map(dest => dest.Name, src => src.chat.Name)
				.Map(dest => dest.LastMessage, src => src.chat.ChatMessages.Count()>0 ? src.chat.ChatMessages.OrderByDescending(x => x.Id).FirstOrDefault().MessageText:"")
				.Map(dest => dest.UnreadMessages, src =>src.chat.ChatMembers.FirstOrDefault(x=>x.UserId==src.UserId).UnreadMessages);
			config.NewConfig<ChatListDto, ChatSearchListDto>()
				.Map(dest => dest.ChatId, src => src.Id)
				.Map(dest => dest.UserId, src => src.ChatMembers.FirstOrDefault().UserId);
			config.NewConfig<(ChatMessage chatMessage,string UserId), ChatMessageListDto>()
				.Map(dest => dest.Id, src => src.chatMessage.Id)
				.Map(dest => dest.UserId, src => src.chatMessage.UserId)
				.Map(dest => dest.SentAt, src => src.chatMessage.SentAt)
				.Map(dest => dest.MessageText, src => src.chatMessage.MessageText)
				.Map(dest => dest.Me, src => src.chatMessage.UserId== src.UserId);
			config.NewConfig<ChatPostDto, Chat>()
				.Map(dest => dest.Id, src =>0)
				.Map(dest => dest.CreatedAt, src =>DateTime.Now)
				.Map(dest => dest.IsGroup, src =>false)
				.Map(dest => dest.Name, src =>"")
				.Map(dest => dest.ChatMembers, src => src.UsersIds.Select(x=>new ChatMember() {UserId=x,CreatedAt= DateTime.Now,UnreadMessages=0,IsAdmin=true,Nickname="" }).ToList());


		}
	}
}
