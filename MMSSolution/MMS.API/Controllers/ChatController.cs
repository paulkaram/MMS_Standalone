using Aspose.Pdf.Operators;
using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.API.Common.Hubs;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.Chat;
using MMS.DTO;
using MMS.DTO.Chats;
using System.Collections.Generic;

namespace MMS.API.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : IntalioBaseController
    {
        private readonly IMainHub _intalioHub;
        private readonly UserManagementManager _userManagementManager;
		private readonly ChatManager _chatManager;
		public ChatController(
            IMainHub hub,
            UserManagementManager userManagementManager ,
            ChatManager chatManager
            )
        {
            _intalioHub = hub;
            _userManagementManager = userManagementManager;
            _chatManager = chatManager;
        }

        [HttpGet("online")]
		[RequiredPermission(PermissionDbEnum.Chat,PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> ListOnlineUsers()
        {
            try
            {
                var connectionsIds =await  _intalioHub.GetConnectedIdsAsync();
                var result = await _userManagementManager.ListUsersByIds(connectionsIds, Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(result));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }


		[HttpGet("list")]
		[RequiredPermission(PermissionDbEnum.Chat, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListUserChats()
		{
			try
			{
                
				var chats =await _chatManager.ListUserChatsAsync(UserId, Language);
				return Ok(new ApiResponseDto<List<ChatListDto>>(chats));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("search")]
		[RequiredPermission(PermissionDbEnum.Chat, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> SearchUsersAndChats()
		{
			try
			{

				var chats = await _chatManager.ListUserAndChatsAsync(UserId, Language);
				return Ok(new ApiResponseDto<List<ChatSearchListDto>>(chats));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}
		[HttpPost]
		[RequiredPermission(PermissionDbEnum.Chat, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> Add(ChatPostDto chatPostDto)
		{
			try
			{
				chatPostDto.UsersIds.Add(UserId);
				if (!chatPostDto.IsGroup)
				{
					bool chatExists = await _chatManager.CheckPrivateChatExist(chatPostDto.UsersIds);
					if (chatExists)
					{
						// Return existing chat instead of creating duplicate
						var existingChats = await _chatManager.ListUserChatsAsync(UserId, Language);
						var existing = existingChats.FirstOrDefault(c =>
							!c.IsGroup && c.ChatMembers.Any(m => chatPostDto.UsersIds.Contains(m.UserId)));
						if (existing != null)
							return Ok(new ApiResponseDto<ChatListDto>(existing));
					}
				}
				ChatListDto chat = await _chatManager.AddChat(UserId, chatPostDto, Language);
				return Ok(new ApiResponseDto<ChatListDto>(chat));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("details/{chatId}")]
		[RequiredPermission(PermissionDbEnum.Chat, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> listChatMessages(int ChatId)
		{
			try
			{
				bool hasAccess=await _chatManager.CheckUserPermission(ChatId, UserId);
				if (hasAccess)
				{
					var messages = await _chatManager.ListChatMessages(UserId, ChatId);
					await _intalioHub.NotifyChatMessagesCount(new string[] { UserId.ToString() });
					return Ok(new ApiResponseDto<List<ChatMessageListDto>>(messages));
				}
				else
				{
					return Unauthorized();
				}
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpPost("chat-message")]
		[RequiredPermission(PermissionDbEnum.Chat, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> AddChatMessage(ChatMessagePostDto chatMessagePostDto)
		{
			try
			{
				bool hasAccess = await _chatManager.CheckUserPermission(chatMessagePostDto.ChatId, UserId);
				if (hasAccess)
				{
					bool added = await _chatManager.AddChatMessage(chatMessagePostDto,UserId);
					if (added)
					{
						var chatMembersIds = await _chatManager.ListChatMembersIdsByChatId(chatMessagePostDto.ChatId);
						await _intalioHub.NotifyChatUsers(chatMembersIds.Select(x => x.ToString()).ToArray());
						await _intalioHub.NotifyChatMessagesCount(chatMembersIds.Where(id => id != UserId).Select(x => x.ToString()).ToArray());
					}
					return Ok(new ApiResponseDto<bool>(added));
				}
				return Unauthorized();
				
				
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("unread-messages-count")]
		public async Task<IActionResult> GetUnreadChatMessagesCount()
		{
			try
			{

				int count = await _chatManager.GetUnreadChatMessagesCount(UserId);
				return Ok(new ApiResponseDto<int>(count));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpPost("meeting-message")]
		public async Task<IActionResult> AddMeetingMessage(ChatMessagePostDto chatMessagePostDto)
		{
			try
			{

				if (chatMessagePostDto.MeetingId == null || chatMessagePostDto.MeetingId == 0)
				{
					return BadRequest();
				}
				bool hasAccess = await _chatManager.CheckUserPermissionForMeeting(chatMessagePostDto.MeetingId.GetValueOrDefault(), UserId);

				if (hasAccess)
				{
					bool added = await _chatManager.AddMeetingMessage(chatMessagePostDto, UserId);
					if (added)
					{
						var meetingMembersIds = await _chatManager.ListMeetingUsersIds(chatMessagePostDto.MeetingId.GetValueOrDefault());
                        meetingMembersIds.Add(UserId);
                        await _intalioHub.NotifyMeetingChatUsers(chatMessagePostDto.MeetingId.Value, meetingMembersIds.Select(x => x.ToString()).ToArray());
					}
					return Ok(new ApiResponseDto<bool>(added));
				}
				else
				{
					return Unauthorized();
				}
				
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("meeting-chat-details/{meetingId}")]
		public async Task<IActionResult> listMeetingChatMessages(int MeetingId)
		{
			try
			{
				bool hasAccess = await _chatManager.CheckUserPermissionForMeeting(MeetingId, UserId);

				if (hasAccess)
				{
					var messages = await _chatManager.ListMeetingChatMessages(UserId, MeetingId, Language);
					return Ok(new ApiResponseDto<List<ChatMessageListDto>>(messages));
				}
				else
				{
					return Unauthorized();
				}
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}
	}
}
