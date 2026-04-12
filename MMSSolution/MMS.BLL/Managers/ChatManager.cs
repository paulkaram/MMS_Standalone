using DocumentFormat.OpenXml.Spreadsheet;
using MapsterMapper;
using Microsoft.IdentityModel.Tokens;
using MMS.DAL.Core.UnitOfWork.Chats;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.Chat;
using MMS.DAL.Models.MMS;
using MMS.DTO.Chats;

namespace MMS.BLL.Managers
{
	public class ChatManager
	{
		private readonly IMapper _mapper;
		private readonly IChatUnitOfWork _chatUnitOfWork;
		private readonly IMMSUnitOfWork _mmsUnitOfWork;
		private readonly IUserManagementUnitOfWork _userManagementUnitOfWork;

		public ChatManager(IMapper mapper,
			IChatUnitOfWork chatUnitOfWork,
			IUserManagementUnitOfWork userManagementUnitOfWork,
			IMMSUnitOfWork mmsUnitOfWork)
		{
			_mapper = mapper;
			_chatUnitOfWork = chatUnitOfWork;
			_userManagementUnitOfWork = userManagementUnitOfWork;
			_mmsUnitOfWork = mmsUnitOfWork;
		}

		public async Task<ChatListDto> AddChat(string userId,ChatPostDto chatPostDto,LanguageDbEnum language)
		{
			var chat = _mapper.Map<Chat>(chatPostDto);
			await _chatUnitOfWork.Chats.AddAsync(chat);
			await _chatUnitOfWork.SaveChangesAsync();
			Chat AddedChat = await _chatUnitOfWork.Chats.FindAsync(chat.Id);
			var chatDto = _mapper.Map<ChatListDto>((AddedChat, userId));
			chatDto.ChatMembers.Remove(chatDto.ChatMembers.FirstOrDefault(x => x.UserId == userId));
			foreach (var member in chatDto.ChatMembers)
			{
				member.Name = _userManagementUnitOfWork.Users.GetFullName(member.UserId.ToString(), language) ?? "";
			}
			if (string.IsNullOrEmpty(chatDto.Name))
			{
				chatDto.Name = chatDto.ChatMembers.FirstOrDefault().Name;
			}
			return chatDto;
		}

		public async Task<bool> AddChatMessage(ChatMessagePostDto chatMessagePostDto,string userId)
		{
			var message = _mapper.Map<ChatMessagePostDto, ChatMessage>(chatMessagePostDto);
			message.UserId= userId;
			await _chatUnitOfWork.ChatMessages.AddAsync(message);
			await _chatUnitOfWork.Chats.UpdateLastChange(message.ChatId, message.SentAt);
			var chatMembers = await _chatUnitOfWork.ChatMembers.ListWithTrackAsync(x =>  x.ChatId == chatMessagePostDto.ChatId&&x.UserId!= userId);
			foreach (var chatMember in chatMembers)
			{
				chatMember.UnreadMessages++;
			}
			return await _chatUnitOfWork.SaveChangesAsync() > 0;
		}

		

		public async Task<bool> CheckPrivateChatExist(List<string> usersIds)
		{
			return await _chatUnitOfWork.Chats.CheckPrivateChatExist(usersIds);
		}

		public async Task<int> GetUnreadChatMessagesCount(string userId)
		{
			return (await _chatUnitOfWork.ChatMembers.ListAsync(x=>x.UserId== userId)).Sum(x=>x.UnreadMessages);
		}

		public async Task<List<string>> ListChatMembersIdsByChatId(int chatId)
		{
			return (await _chatUnitOfWork.ChatMembers.ListAsync(x => x.ChatId == chatId)).Select(x => x.UserId).ToList();
		}

		public async Task<List<ChatMessageListDto>> ListChatMessages(string UserId, int chatId)
		{
			IEnumerable<ChatMessage> messages = await _chatUnitOfWork.ChatMessages.ListAsync(x => x.ChatId == chatId);
			var chatMember=await _chatUnitOfWork.ChatMembers.GetAsync(x=>x.UserId==UserId&& x.ChatId==chatId);
			if (chatMember.UnreadMessages > 0)
			{
				chatMember.UnreadMessages = 0;
				await _chatUnitOfWork.SaveChangesAsync();
			}
			return messages.Select(x => _mapper.Map<ChatMessageListDto>((x, UserId))).ToList();
		}

		public async Task<List<ChatMessageListDto>?> ListMeetingChatMessages(string UserId, int meetingId, LanguageDbEnum language)
		{
			Chat? chat = await _chatUnitOfWork.Chats.GetAsync(x => x.MeetingId == meetingId);

			if(chat == null)
			{
				return new List<ChatMessageListDto>();
			}
			IEnumerable<ChatMessage> messages = await _chatUnitOfWork.ChatMessages.ListAsync(x => x.ChatId == chat.Id);
			
			var messagesList = messages.Select(x => _mapper.Map<ChatMessageListDto>((x, UserId))).ToList();

			var distinctUserIds = messages.Select(m => m.UserId).Distinct().ToList();
			var users=await _userManagementUnitOfWork.Users.ListAsync(x=> distinctUserIds.Contains(x.Id));
			var userDict =
				users.ToDictionary(u => u.Id, u =>
				(u.HasProfilePicture,name: language == LanguageDbEnum.Arabic ? u.FullnameAr : u.FullnameEn));

			foreach (var message in messagesList)
			{
				if (userDict.TryGetValue(message.UserId, out var user))
				{
					message.HasProfilePicture = user.HasProfilePicture;
					message.UserName= user.name;
				}
			}
			return messagesList;

		}
		public async Task<bool> AddMeetingMessage(ChatMessagePostDto chatMessagePostDto,string userId)
		{
			Chat? chat = await _chatUnitOfWork.Chats.GetAsync(x => x.MeetingId == chatMessagePostDto.MeetingId);
			if (chat == null)
			{
				chat = new Chat()
				{
					MeetingId = chatMessagePostDto.MeetingId,
					CreatedAt = DateTime.Now,
					Name = "",
					IsGroup = true,

				};
				await _chatUnitOfWork.Chats.AddAsync(chat);
				await _chatUnitOfWork.SaveChangesAsync();
			}
			chatMessagePostDto.ChatId = chat.Id;
			var message = new ChatMessage()
			{
				Id=0,
				ChatId=chat.Id,
				MessageText=chatMessagePostDto.MessageText,
				UserId=userId,
				SentAt=DateTime.Now,

			};
			await _chatUnitOfWork.ChatMessages.AddAsync(message);
			await _chatUnitOfWork.Chats.UpdateLastChange(message.ChatId, message.SentAt);
			return await _chatUnitOfWork.SaveChangesAsync() > 0;
		}
		public async Task<List<string>> ListMeetingUsersIds(int MeetingId)
		{
			return (await _mmsUnitOfWork.MeetingAttendees.ListAsync(x => x.MeetingId == MeetingId)).Select(x => x.UserId).ToList();
		}

		public async Task<List<ChatSearchListDto>?> ListUserAndChatsAsync(string userId, LanguageDbEnum language)
		{
			List<ChatSearchListDto> list = new List<ChatSearchListDto>();
			var chats = await ListUserChatsAsync(userId, language);
			var users = await _userManagementUnitOfWork.Users.ListAsync();
			list.AddRange(chats.Select(chat => new ChatSearchListDto() { ChatId = chat.Id, UserId = chat.ChatMembers.First().UserId, Name = chat.Name }));
			foreach (var user in users)
			{
				if (!chats.Any(c => c.ChatMembers.Any(m => m.UserId == user.Id) && !c.IsGroup))
				{

					list.Add(new ChatSearchListDto() { ChatId = 0, UserId = user.Id, Name = language == LanguageDbEnum.Arabic ? user.FullnameAr : user.FullnameEn });
				}
			}
			return list;
		}

		public async Task<List<ChatListDto>> ListUserChatsAsync(string userId, LanguageDbEnum language)
		{
			var chats = await _chatUnitOfWork.Chats.ListIncludeChatMembersAsync(x =>
			x.ChatMembers.Any(m => m.UserId == userId),
			orderBy: x => x.UpdatedAt);

			if (chats != null)
			{

				var chatDto = chats.Select(x =>
				{
					var chatDto = _mapper.Map<ChatListDto>((x,userId));
					chatDto.ChatMembers.Remove(chatDto.ChatMembers.FirstOrDefault(x => x.UserId == userId));
					foreach (var member in chatDto.ChatMembers)
					{
						member.Name = _userManagementUnitOfWork.Users.GetFullName(member.UserId.ToString(), language) ?? "";
					}
					if (string.IsNullOrEmpty(chatDto.Name))
					{
						chatDto.Name = chatDto.ChatMembers.FirstOrDefault().Name;
					}
					return chatDto;
				}).ToList();

				return chatDto;
			}
			return new List<ChatListDto>();
		}

		public async Task<bool> CheckUserPermission(int chatId, string userId)
		{
			return await _chatUnitOfWork.ChatMembers.AnyAsync(x=>x.ChatId == chatId && x.UserId == userId);
		}

		public async Task<bool> CheckUserPermissionForMeeting(int meetingId, string userId)
		{
			bool isOwner =await  _mmsUnitOfWork.Meetings.AnyAsync(x => x.Id == meetingId && x.Createdby == userId);//the owner may not be in the attendees
			if (isOwner)
			{
				return true;
			}
			return await _mmsUnitOfWork.MeetingAttendees.AnyAsync(x => x.MeetingId == meetingId && x.UserId == userId);
		}
	}
}
