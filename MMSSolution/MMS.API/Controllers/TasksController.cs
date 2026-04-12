using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.API.Common.Hubs;
using MMS.BLL.Constants;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DTO.Tasks;

namespace MMS.API.Controllers
{
	[Route("api/tasks")]
	public class TasksController : IntalioBaseController
	{
		private readonly MeetingManager _meetingManager;
		private readonly AttachmentManager _attachmentManager;
		private readonly IMainHub _intalioHub;

		public TasksController(MeetingManager meetingManager, AttachmentManager attachmentManager, IMainHub hub)
		{
			_meetingManager = meetingManager;
			_attachmentManager = attachmentManager;
			_intalioHub = hub;
		}

		/// <summary>
		/// List meeting tasks for the current user (pending approval).
		/// </summary>
		[HttpGet("meeting")]
		[RequiredPermission(PermissionDbEnum.MMSTasks, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListMeetingTasks(int page = 1, int pageSize = PaginationConstants.DefaultPageSize)
		{
			try
			{
				(page, pageSize) = PaginationConstants.ValidatePagination(page, pageSize);
				var result = await _meetingManager.ListMeetingTasksAsync(UserId, page, pageSize, Language);
				return Ok(new ApiResponseDto<GenericPaginationListDto<MeetingTaskListItemDto>>(
					result ?? new GenericPaginationListDto<MeetingTaskListItemDto>(0, new List<MeetingTaskListItemDto>())));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		/// <summary>
		/// Claim a meeting task (mark as viewed).
		/// </summary>
		[HttpPost("{taskId}/meeting/claim")]
		public async Task<IActionResult> ClaimMeetingTask(int taskId)
		{
			try
			{
				bool hasAccess = await _meetingManager.IsTaskOwnerAsync(UserId, taskId);
				if (!hasAccess) return Unauthorized();

				bool isClaimed = await _meetingManager.ClaimMeetingTaskAsync(taskId, UserId);
				if (isClaimed)
				{
					await _intalioHub.ClaimTask(UserId.ToString());
				}
				return Ok(new ApiResponseDto<object>(Success: true));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		/// <summary>
		/// Approve or reject a meeting task.
		/// </summary>
		[HttpPost("{taskId}/meeting/approve")]
		public async Task<IActionResult> ApproveMeetingTask(int taskId, bool approved, [FromBody] string note)
		{
			try
			{
				bool hasAccess = await _meetingManager.IsTaskOwnerAsync(UserId, taskId);
				if (!hasAccess) return Unauthorized();

				var (success, message) = await _meetingManager.ApproveMeetingTaskAsync(taskId, UserId, approved, note);
				return Ok(new ApiResponseDto<string>(Data: message, Success: success, Message: message));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		/// <summary>
		/// Get attachment query string for a meeting task with signing permissions.
		/// </summary>
		[HttpGet("{taskId}/meeting/attachment/{attachmentId}")]
		public async Task<IActionResult> GetMeetingTaskAttachment(int taskId, int attachmentId)
		{
			try
			{
				bool hasAccess = await _meetingManager.IsTaskOwnerAsync(UserId, taskId);
				if (!hasAccess) return Unauthorized();

				bool isTaskCompleted = await _meetingManager.IsTaskCompletedAsync(taskId);
				var attachmentAction = new AttachmentActionDto { Sign = !isTaskCompleted };

				string? attachmentQuery = await _attachmentManager.GetAttachmentQuery(
					attachmentId, UserId, attachmentAction, taskId);

				return Ok(new ApiResponseDto<string?>(
					Data: attachmentQuery,
					Success: !string.IsNullOrWhiteSpace(attachmentQuery)));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}
	}
}
