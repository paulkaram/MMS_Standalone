using Intalio.Tools.Common.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.API.Common.Hubs;
using MMS.BLL.Constants;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.Meetings;


namespace MMS.API.Controllers
{
    /// <summary>
    /// Meetings controller.
    /// DCC Compliance (NCA DCC-1:2022 Section 2-4): Meeting actions are audited.
    /// </summary>
	[Route("api/meetings")]
	[ApiController]
	public class MeetingsController : IntalioBaseController
	{
		private readonly MeetingManager _meetingManager;
		private readonly IMainHub _intalioHub;
		private readonly EmailNotificationManager _emailNotificationManager;
		private readonly MmsNotificationService _notify;

		public MeetingsController(MeetingManager meetingManager, EmailNotificationManager emailNotificationManager,
						IMainHub hub, MmsNotificationService notify)
		{
			_meetingManager = meetingManager;
			_intalioHub = hub;
			_emailNotificationManager = emailNotificationManager;
			_notify = notify;
		}

		[HttpPost("{meetingId}/send-meeting")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Write)]
		[LogUserActivity(AuditOperationConstants.MeetingAction, "Sent meeting invitation for meeting {meetingId}")]
		public async Task<IActionResult> SendMeeting(int meetingId)
		{
			try
			{
				bool hasAccess =await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess) {
					await _meetingManager.SendMeetingAsync(meetingId);

					var meeting = await _meetingManager.GetMeetingInfoForInvitation(meetingId);
					var meetingAttendeesFullInfo = await _meetingManager.GetMeetingInfoForInvitationAttendeesFullInfo(meetingId);
                    //await _emailNotificationManager.SendMeetingInvitation(meeting, meetingId, Language);
					await _emailNotificationManager.SendMeetingRequest(meeting, meetingAttendeesFullInfo, Language);

					// IAM Notification Center
					_ = _notify.MeetingInviteSent(
						meeting?.Title ?? "",
						meeting?.Date ?? DateTime.Today,
						meeting?.StartTime ?? "",
						meetingAttendeesFullInfo?.Select(a => a.UserId).ToList() ?? new(),
						UserId);

					return Ok(new ApiResponseDto<object>(Success: true));
				}
				else
				{
					return Unauthorized();
				}
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpPost("{meetingId}/approve-meeting")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Write)]
		[LogUserActivity(AuditOperationConstants.Approve, "Approved meeting {meetingId}")]
		public async Task<IActionResult> ApproveMeeting(int meetingId)
		{
			try
			{
				bool hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{
					await _meetingManager.ApproveMeetingAsync(meetingId);
					return Ok(new ApiResponseDto<object>(Success: true));
				}
				else
				{
					return Unauthorized();
				}
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpGet("{meetingId}")]
		public async Task<IActionResult> GetMeeting(int meetingId)
		{
			try
			{
				bool hasAccess = await _meetingManager.IsMeetingMember(UserId, meetingId);
				if (hasAccess)
				{
					var meeting = await _meetingManager.GetMeetingAsync(meetingId, UserId, Language);
					return Ok(new ApiResponseDto<MeetingPostDto>(meeting));
				}
				else
				{
					return Unauthorized();
				}
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpGet("{meetingId}/live-meeting")]
		public async Task<IActionResult> GetLiveMeetingDetails(int meetingId)
		{
			try
			{
				bool hasAccess = await _meetingManager.IsMeetingMember(UserId, meetingId);
				if (hasAccess)
				{
					var meeting = await _meetingManager.GetLiveMeetingDetails(meetingId, UserId, Language);
					return Ok(new ApiResponseDto<LiveMeetingDto>(meeting));
				}
				else { 
					return Unauthorized();
				}
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpGet("{meetingId}/attendees")]
		[RequiredPermission(PermissionDbEnum.Meetings, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListMeetingAttendees(int meetingId)
		{
			try
			{
				bool hasAccess = await _meetingManager.IsMeetingMember(UserId, meetingId);
				if (hasAccess)
				{
					var attendees = await _meetingManager.ListMeetingAttendeesAsync(meetingId, Language);
					return Ok(new ApiResponseDto<List<MeetingAttendeeListItemDto>>(attendees));
				}
				else
				{
					return Unauthorized();
				}
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpGet("{meetingId}/agendas")]
		[RequiredPermission(PermissionDbEnum.Meetings, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListMeetingAgendas(int meetingId)
		{
			try
			{
				bool hasAccess = await _meetingManager.IsMeetingMember(UserId, meetingId);
				if (hasAccess)
				{
					var agendas = await _meetingManager.ListMeetingAgendasAsync(meetingId, Language);
					return Ok(new ApiResponseDto<List<MeetingAgendaListItemDto>>(agendas));
				}
				else
				{
					return Unauthorized();
				}
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpGet("user/drafts/{Page}/{PageSize}")]
		[RequiredPermission(PermissionDbEnum.Meetings, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListUserDraftMeetings(int Page = 1, int PageSize = PaginationConstants.DefaultPageSize)
		{
			try
			{
				(Page, PageSize) = PaginationConstants.ValidatePagination(Page, PageSize);
				var userCommittees = await _meetingManager.ListUserDraftMeetingsAsync(UserId, Page, PageSize, Language);

				return Ok(new ApiResponseDto<GenericPaginationListDto<MeetingListItemDto>>(userCommittees));
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}


		[HttpPost("calender-search")]
		public async Task<IActionResult> SearchCalenderMeetings(SearchCalenderMeetingDto searchMeetingDto)
		{
			try
			{
				var userMeetings = await _meetingManager.ListUserMeetingsForCalenderAsync(UserId, searchMeetingDto);

				return Ok(new ApiResponseDto<List<CalenderMeetingDto>>(userMeetings));
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpPost("search/{Page}/{PageSize}")]
		public async Task<IActionResult> SearchMeetings(SearchMeetingDto searchMeetingDto, int Page = 1, int PageSize = PaginationConstants.DefaultPageSize)
		{
			try
			{
				(Page, PageSize) = PaginationConstants.ValidatePagination(Page, PageSize);
				var userMeetings = await _meetingManager.ListUsertMeetingsAsync(searchMeetingDto, UserId, Page, PageSize, Language);

				return Ok(new ApiResponseDto<GenericPaginationListDto<MeetingListItemDto>>(userMeetings));
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpDelete("{meetingId}")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Full)]
		[LogUserActivity(AuditOperationConstants.Delete, "Deleted meeting {meetingId}")]
		public async Task<IActionResult> DeleteMeeting(int meetingId)
		{
			try
			{
				bool hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{
					await _meetingManager.DeleteMeetingAsync(meetingId);
					return Ok(new ApiResponseDto<object>(Success: true));
				}
				else
				{
					return Unauthorized();
				}
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpGet]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> ListMeetings(string search, SearchTypeEnum mode = SearchTypeEnum.List)
		{
			try
			{
				switch (mode)
				{
					case SearchTypeEnum.Autocomplete:
						var meetings = await _meetingManager.ListMeetingsForAutoCompleteAsync(search, UserId);
						return Ok(new ApiResponseDto<List<AssociatedMeetingDto>>(meetings));
					default:
						return Ok(new ApiResponseDto<List<AssociatedMeetingDto>>(new()));
				}
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpPut("{meetingId}/next-agenda")]
		public async Task<IActionResult> StartNextMeetingAgenda(int meetingId)
		{
			try
			{
				var hasAccessToUpdate = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccessToUpdate) {
					var res = await _meetingManager.StartNextMeetingAgenda(meetingId, Language);
					if (res.success)
					{
						var usersIds = await _meetingManager.ListMeetingAttendeesIds(meetingId);
						//check if new agenda begins
						if (res.newAgendaStartedId != null)
						{
							var meetingTitle = await _meetingManager.GetMeetingNameAsync(meetingId);
							await _intalioHub.NotifyNewMeetingAgendaBegins(usersIds.Select(x => x.ToString()).ToArray(), res.agendaItems.Where(x => x.Id == res.newAgendaStartedId.Value).FirstOrDefault(), meetingTitle);
						}
						await _intalioHub.NotifyMeetingAgendaChange(usersIds.Select(x => x.ToString()).ToArray(), meetingId, res.agendaItems);
						if (!usersIds.Any(x => x == UserId))
						{
							usersIds.Add(UserId);
						}
						if (res.newStatus != null)
						{
							await _intalioHub.NotifyMeetingStatusChange(usersIds.Select(x => x.ToString()).ToArray(),meetingId, res.newStatus.GetValueOrDefault());
						}
					}

					return Ok(new ApiResponseDto<bool>(Success: true));
				}
				return Unauthorized();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpPut("{meetingId}/pause-resume")]
		public async Task<IActionResult> StartMeeting(int meetingId)
		{
			try
			{
				var hasAccessToUpdate = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccessToUpdate) {
					var res = await _meetingManager.PauseMeetingAgenda(meetingId, Language);
					if (res.success)
					{
						List<string> usersIds = await _meetingManager.ListMeetingAttendeesIds(meetingId);
						if (!usersIds.Contains(UserId))
						{
							usersIds.Add(UserId);
						}
						await _intalioHub.NotifyMeetingAgendaChange(usersIds.ToArray(), meetingId, res.agendaItems);
					}
					return Ok(new ApiResponseDto<bool>(Success: true));
				}

				return Unauthorized();				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpGet("{meetingId}/initial-meeting-minutes")]
		public async Task<IActionResult> GetInitialMeetingMiniutes(int meetingId)
		{
			try
			{
				var hasAccessToView = await _meetingManager.CanViewMeetingMinutes(UserId, meetingId);
				if (hasAccessToView)
				{
					var attachment = await _meetingManager.GetMeetingMinutesFile(meetingId);
				
					return Ok(new ApiResponseDto<AttachmentListItemDto>(attachment));
				}
				return Unauthorized();

			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpGet("{meetingId}/final-meeting-minutes")]
		public async Task<IActionResult> GetFinalMeetingMiniutes(int meetingId)
		{
			try
			{
				var hasAccessToView = await _meetingManager.CanViewFinalwMeetingMinutes(UserId, meetingId);
				if (hasAccessToView)
				{
					var attachment = await _meetingManager.GetFinalMeetingMinutesFile(meetingId);

					return Ok(new ApiResponseDto<AttachmentListItemDto>(attachment));
				}
				return Unauthorized();

			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}


		[HttpPost("{meetingId}/meeting-minutes-generate")]
		public async Task<IActionResult> GenerateMeetingMiniutes(int meetingId)
		{
			try
			{
				var hasAccessToView = await _meetingManager.CanViewMeetingMinutes(UserId, meetingId);
				if (hasAccessToView) {
					var res = await _meetingManager.GenerateMeetingMinutesFile(meetingId, UserId, Language);
					if (res.newStatus != null)
					{
						var usersIds = await _meetingManager.ListMeetingAttendeesIds(meetingId);
						if (!usersIds.Any(x => x == UserId))
						{
							usersIds.Add(UserId);
						}
						await _intalioHub.NotifyMeetingStatusChange(usersIds.Select(x => x.ToString()).ToArray(), meetingId, res.newStatus.Value);

					}
					return Ok(new ApiResponseDto<AttachmentListItemDto>(res.attachment));
				}
				return Unauthorized();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpPost("{meetingId}/upload-initial-meeting-minutes")]
		public async Task<IActionResult> UploadMeetingMiniutes(int meetingId)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{

					FileStatusEnum status = FileStatusEnum.Valid;
						IFormCollection formCollection = await Request.ReadFormAsync();
						if (formCollection.Files.Count > 0)
						{
							(status, _) = base.AreValidAttachments(formCollection.Files);

						}
						else
						{
							return BadRequest();
						}

					if (status != FileStatusEnum.Valid)
						{
							return Ok(new ApiResponseDto<string>(Success: false, Message: status.GetDisplayName()));
						}
						var res = await _meetingManager.UploadInitialMeetingMinutes(meetingId,UserId, formCollection.Files, Language);


					if (res.newStatus != null)
					{
						var usersIds = await _meetingManager.ListMeetingAttendeesIds(meetingId);
						if (!usersIds.Any(x => x == UserId))
						{
							usersIds.Add(UserId);
						}
						await _intalioHub.NotifyMeetingStatusChange(usersIds.Select(x => x.ToString()).ToArray(), meetingId, res.newStatus.Value);

					}
					return Ok(new ApiResponseDto<AttachmentListItemDto>(res.attachment));
				}
				return Unauthorized();

			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpPost("{meetingId}/upload-final-meeting-minutes")]
		public async Task<IActionResult> UploadFinalMeetingMiniutes(int meetingId)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{

					FileStatusEnum status = FileStatusEnum.Valid;
					IFormCollection formCollection = await Request.ReadFormAsync();
					if (formCollection.Files.Count > 0)
					{
						(status, _) = base.AreValidAttachments(formCollection.Files);

					}
					else
					{
						return BadRequest();
					}

					if (status != FileStatusEnum.Valid)
					{
						return Ok(new ApiResponseDto<string>(Success: false, Message: status.GetDisplayName()));
					}
					var res = await _meetingManager.UploadFinalMeetingMinutes(meetingId, UserId, formCollection.Files, Language);


					if (res.newStatus != null)
					{
						var usersIds = await _meetingManager.ListMeetingAttendeesIds(meetingId);
						if (!usersIds.Any(x => x == UserId))
						{
							usersIds.Add(UserId);
						}
						await _intalioHub.NotifyMeetingStatusChange(usersIds.Select(x => x.ToString()).ToArray(), meetingId, res.newStatus.Value);

					}
					return Ok(new ApiResponseDto<AttachmentListItemDto>(res.attachment));
				}
				return Unauthorized();

			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpPost("send-initial-Meeting-Minutes")]
		public async Task<IActionResult> SendMeetingMiniutesForApproval(MeetingMinutesTaskDto meetingMinutesTaskDto)
		{
			try
			{
				var hasAccessToUpdate = await _meetingManager.IsMeetingOwner(UserId, meetingMinutesTaskDto.MeetingId);
				if (hasAccessToUpdate) {
					var res = await _meetingManager.SendMeetingMinutesTaskForApproval(meetingMinutesTaskDto, TaskTypeDbEnum.InitialMeetingMinutesApproval);
					await _intalioHub.NotifyUsers(meetingMinutesTaskDto.UsersIds.Select(x => x.ToString()).ToArray());
					return Ok(new ApiResponseDto<bool>(res));
				}
				return Unauthorized();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpPost("send-final-Meeting-Minutes")]
		public async Task<IActionResult> SendFinalMeetingMiniutesForApproval(MeetingMinutesTaskDto meetingMinutesTaskDto)
		{
			try
			{
				var hasAccessToUpdate = await _meetingManager.IsMeetingOwner(UserId, meetingMinutesTaskDto.MeetingId);
				if (hasAccessToUpdate) {
					var res = await _meetingManager.SendFinalMeetingMinutesTaskForSign(meetingMinutesTaskDto);
					await _intalioHub.NotifyUsers(meetingMinutesTaskDto.UsersIds.Select(x => x.ToString()).ToArray());
					return Ok(new ApiResponseDto<bool>(res));
				}
			
				return Unauthorized();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpGet("{meetingId}/initial-meeting-minutes-users")]
		public async Task<IActionResult> GetInitialMeetingMinutesUsers(int meetingId)
		{
			try
			{
				var hasAccessToUpdate = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccessToUpdate) {
					var res = await _meetingManager.GetInitialMeetingMinutesUsers(meetingId);
					return Ok(new ApiResponseDto<List<string>>(res));
				}

				return Unauthorized();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpGet("{meetingId}/final-meeting-minutes-users")]
		public async Task<IActionResult> GetFinalMeetingMinutesUsers(int meetingId)
		{
			try
			{
				var hasAccessToUpdate = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccessToUpdate) {
					var res = await _meetingManager.GetFinalMeetingMinutesUsers(meetingId);
					return Ok(new ApiResponseDto<List<string>>(res));
				}

				return Unauthorized();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
        [HttpPost("{meetingId}/cancelMeeting")]
        [LogUserActivity(AuditOperationConstants.MeetingAction, "Cancelled meeting {meetingId}")]
        public async Task<IActionResult> CancelMeeting(int meetingId)
		{
            try
            {
                var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
                if (hasAccess)
                {
                    var meeting = await _meetingManager.GetMeetingInfoForInvitation(meetingId);
                    var attendees = await _meetingManager.GetMeetingInfoForInvitationAttendeesFullInfo(meetingId);

                    await _meetingManager.CancelMeeting(meetingId);

                    _ = _notify.MeetingCanceled(
                        meeting?.Title ?? "",
                        meeting?.Date ?? DateTime.Today,
                        attendees?.Select(a => a.UserId).ToList() ?? new(),
                        UserId);

                    return Ok(new ApiResponseDto<object>(Success: true));
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        /// <summary>
        /// Test endpoint to send Outlook calendar invite for a meeting.
        /// Use this to verify Outlook integration is working correctly.
        /// </summary>
        [HttpPost("{meetingId}/test-outlook-invite")]
        [RequiredPermission(PermissionDbEnum.SystemSettings, PermissionLevelDbEnum.Write)]
        public async Task<IActionResult> TestOutlookInvite(int meetingId)
        {
            try
            {
                var meeting = await _meetingManager.GetMeetingWithAttendeesAsync(meetingId);
                if (meeting == null)
                {
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: "Meeting not found"));
                }

                _emailNotificationManager.SendMeetingCalendarInvite(meeting);

                return Ok(new ApiResponseDto<object>(
                    Success: true,
                    Message: $"Outlook calendar invite sent to {meeting.MeetingAttendees?.Count ?? 0} attendees"
                ));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost("{meetingId}/final-meeting-minutes-generate")]
		public async Task<IActionResult> GenerateFinalMeetingMiniutes(int meetingId)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess) {
					var res = await _meetingManager.GenerateFinalMeetingMinutesFile(meetingId, UserId, Language);
					if (res.newStatus != null)
					{
						var usersIds = await _meetingManager.ListMeetingAttendeesIds(meetingId);
						if (!usersIds.Any(x => x == UserId))
						{
							usersIds.Add(UserId);
						}
						await _intalioHub.NotifyMeetingStatusChange(usersIds.Select(x => x.ToString()).ToArray(), meetingId, res.newStatus.Value);
					}
					return Ok(new ApiResponseDto<AttachmentListItemDto>(res.attachment));
				}
				return Unauthorized();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpPut("{meetingId}/approve-initial-meeting-minutes")]
		[LogUserActivity(AuditOperationConstants.Approve, "Approved initial meeting minutes for meeting {meetingId}")]
		public async Task<IActionResult> ApproveInitialMeetingMinutes(int meetingId)
		{
			try
			{
				var hasAccessToUpdate = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccessToUpdate) {
					bool res = await _meetingManager.ApproveInitialMeetingMinutes(meetingId);
					if (res)
					{
						var usersIds = await _meetingManager.ListMeetingAttendeesIds(meetingId);
						if (!usersIds.Any(x => x == UserId))
						{
							usersIds.Add(UserId);
						}
						await _intalioHub.NotifyMeetingStatusChange(usersIds.Select(x => x.ToString()).ToArray(), meetingId, MeetingStatusDbEnum.InitialMeetingMinutesApproved);
					}
					return Ok(new ApiResponseDto<bool>(Success: true));
				}
				return Unauthorized();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpPut("{meetingId}/approve-final-meeting-minutes")]
		[LogUserActivity(AuditOperationConstants.Approve, "Approved final meeting minutes for meeting {meetingId}")]
		public async Task<IActionResult> ApproveFinalMeetingMinutes(int meetingId)
		{
			try
			{
				var hasAccessToUpdate = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccessToUpdate) {
					bool res = await _meetingManager.ApproveFinalMeetingMinutes(meetingId);
					if (res)
					{
						var usersIds = await _meetingManager.ListMeetingAttendeesIds(meetingId);
						if (!usersIds.Any(x => x == UserId))
						{
							usersIds.Add(UserId);
						}
						await _intalioHub.NotifyMeetingStatusChange(usersIds.Select(x => x.ToString()).ToArray(), meetingId, MeetingStatusDbEnum.FinalMeetingMinutesSigned);

						// Return success with new status ID
						return Ok(new ApiResponseDto<object>(new {
							success = true,
							newMeetingStatusId = (int)MeetingStatusDbEnum.FinalMeetingMinutesSigned
						}));
					}

					return Ok(new ApiResponseDto<bool>(res));
				}
				else
				{
					return Unauthorized();
				}
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpPut("user-attend")]
		public async Task<IActionResult> UpdateUserAttend(UserAttendPutDto userAttendPutDto)
		{
			try
			{
				var hasAccessToUpdate = await _meetingManager.IsMeetingOwner(UserId, userAttendPutDto.MeetingId);
				if (hasAccessToUpdate) {
					var res = await _meetingManager.UpdateUserAttend(userAttendPutDto, Language);
					return Ok(new ApiResponseDto<List<MeetingAttendeePostDto>>(res));
				}
				return Unauthorized();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpPost("{meetingId}/attendees/{attendeeId}/check-in")]
		public async Task<IActionResult> CheckInAttendee(int meetingId, string attendeeId)
		{
			try
			{
				var hasAccessToUpdate = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccessToUpdate)
				{
					var userAttendPutDto = new UserAttendPutDto(attendeeId, meetingId, true);
					var res = await _meetingManager.UpdateUserAttend(userAttendPutDto, Language);
					return Ok(new ApiResponseDto<List<MeetingAttendeePostDto>>(res));
				}
				return Unauthorized();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpPost("{meetingId}/attendees/{attendeeId}/check-out")]
		public async Task<IActionResult> CheckOutAttendee(int meetingId, string attendeeId)
		{
			try
			{
				var hasAccessToUpdate = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccessToUpdate)
				{
					var userAttendPutDto = new UserAttendPutDto(attendeeId, meetingId, false);
					var res = await _meetingManager.UpdateUserAttend(userAttendPutDto, Language);
					return Ok(new ApiResponseDto<List<MeetingAttendeePostDto>>(res));
				}
				return Unauthorized();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpGet("{meetingId}/final-meeting-minutes-versions")]
		public async Task<IActionResult> GetFinalMeetingMinutesVersions(int meetingId)
		{
			try
			{
				var hasAccess = await _meetingManager.CanViewFinalwMeetingMinutes(UserId, meetingId);
				if (hasAccess)
				{
					var versions = await _meetingManager.GetFinalMeetingMinutesVersions(meetingId);
					return Ok(new ApiResponseDto<List<AttachmentListItemDto>>(versions));
				}
				else
				{
					return Unauthorized();
				}
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpGet("{meetingId}/initial-meeting-minutes-versions")]
		public async Task<IActionResult> GetInitialMeetingMinutesVersions(int meetingId)
		{
			try
			{
				var hasAccess = await _meetingManager.CanViewMeetingMinutes(UserId, meetingId);
				if (hasAccess)
				{
					var versions = await _meetingManager.GetInitialMeetingMinutesVersions(meetingId);
					return Ok(new ApiResponseDto<List<AttachmentListItemDto>>(versions));
				}

				return Unauthorized();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpGet("{meetingId}/meeting-user-approvals")]
		public async Task<IActionResult> GetMeetingUsersApprovals(int meetingId)
		{
			try
			{
				var hasAccessToUpdate = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccessToUpdate) {
					var approvals = await _meetingManager.GetMeetingApprovals(meetingId, Language);
					return Ok(new ApiResponseDto<List<MeetingUserApprovalDto>>(approvals));
				}
				return Unauthorized();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpGet("{meetingId}/meeting-minutes-users-sign")]
		public async Task<IActionResult> GetFinalMeetingSigns(int meetingId)
		{
			try
			{
				var hasAccessToUpdate = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccessToUpdate) {
					var approvals = await _meetingManager.GetMeetingMinutesSignatures(meetingId, Language);
					return Ok(new ApiResponseDto<List<MeetingUserApprovalDto>>(approvals));
				}
				return Unauthorized();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpGet("{attachmentId}/meeting-minutes-users-approvals")]
		public async Task<IActionResult> GetInitialMeetingMinutesApprovals(int attachmentId)
		{
			try
			{
				var hasAccess = await _meetingManager.CanViewMeetingMinutesApprovals(UserId, attachmentId);
				if (hasAccess)
				{
					var approvals = await _meetingManager.GetMeetingMinutesApprovals(attachmentId, Language);
					return Ok(new ApiResponseDto<List<MeetingUserApprovalDto>>(approvals));
				}
				return Unauthorized();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpPut("{meetingId}/end-meeting")]
		public async Task<IActionResult> EndMeeting(int meetingId)
		{
			try
			{
				var hasAccessToUpdate = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccessToUpdate)
				{
					var res = await _meetingManager.EndMeeting(meetingId, Language);
					if (res.success)
					{
						var usersIds = await _meetingManager.ListMeetingAttendeesIds(meetingId);
						await _intalioHub.NotifyMeetingAgendaChange(usersIds.Select(x => x.ToString()).ToArray(), meetingId, res.agendaItems);
						if (!usersIds.Any(x => x == UserId))
						{
							usersIds.Add(UserId);
						}
						await _intalioHub.NotifyMeetingStatusChange(usersIds.Select(x => x.ToString()).ToArray(), meetingId, MeetingStatusDbEnum.Finished);

						return Ok(new ApiResponseDto<bool>(true));
					}
					return Ok(new ApiResponseDto<bool>(false,Success:false));
				}
				else
				{
					return Unauthorized();
				}
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		#region New Add Meeting Apis

		[HttpPost("meeting-info")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Write)]
		[LogUserActivity(AuditOperationConstants.Create, "Created new meeting")]
		public async Task<IActionResult> AddMeetingInfo(MeetingInfoPostDto meetingInfoPostDto)
		{
			try
			{
				var info = await _meetingManager.AddMeetingInfo(meetingInfoPostDto, UserId,Language);
				if (info != null)
				{
					return Ok(new ApiResponseDto<MeetingInfoDto>(info));
				}
				return BadRequest();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpGet("{meetingId}/meeting-info")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetMeetingInfo(int meetingId)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingMember(UserId, meetingId);
				var info = await _meetingManager.GetMeetingInfo(meetingId, Language);

				return Ok(new ApiResponseDto<MeetingInfoDto>(info));
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpPut("meeting-info")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Write)]
		[LogUserActivity(AuditOperationConstants.Update, "Updated meeting {Id}")]
		public async Task<IActionResult> UpdateMeetingInfo(MeetingInfoPostDto meetingInfoPostDto)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingInfoPostDto.Id);
				if (hasAccess)
				{
					MeetingInfoDto info = await _meetingManager.UpdateMeetingInfo(meetingInfoPostDto, Language);
					if (info!=null)
					{
						return Ok(new ApiResponseDto<MeetingInfoDto>(info));
					}
					else
					{
						return BadRequest();
					}
				}
				return Unauthorized();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpPost("{meetingId}/meeting-attendee")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> AddMeetingAttendee(int meetingId, MeetingAttendeePostDto attendee)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{
					List<MeetingAttendeePostDto> attendeesList = await _meetingManager.AddMeetingAttendee(meetingId, attendee, Language);
					if (attendeesList != null) {
						return Ok(new ApiResponseDto<List<MeetingAttendeePostDto>>(attendeesList));
					}
					return ConflictResponse("UserExistInMeeting");

				}
				return Unauthorized();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpPut("{meetingId}/meeting-attendee")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> UpdateMeetingAttendee(int meetingId, MeetingAttendeePostDto attendee)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{
					List<MeetingAttendeePostDto> attendeesList = await _meetingManager.UpdateMeetingAttendee(meetingId, attendee, Language);
					return Ok(new ApiResponseDto<List<MeetingAttendeePostDto>>(attendeesList));
				}
				return Unauthorized();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpDelete("{meetingId}/meeting-attendee/{userId}")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> DeleteMeetingAttendee(int meetingId, string userId)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{
					List<MeetingAttendeePostDto> attendeesList = await _meetingManager.DeleteMeetingAttendee(meetingId, userId, Language);
					return Ok(new ApiResponseDto<List<MeetingAttendeePostDto>>(attendeesList));
				}
				return Unauthorized();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpPost("{meetingId}/meeting-agenda")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> AddMeetingAgenda(int meetingId, MeetingAgendaPostDto meetingAgendaPostDto)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{
					var agendas = await _meetingManager.AddMeetingAgenda(meetingAgendaPostDto, meetingId, UserId, Language);

					return Ok(new ApiResponseDto<List<MeetingAgendaListItemDto>>(agendas));
				}
				return Unauthorized();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpPut("{meetingId}/meeting-agenda")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> UpdateMeetingAgenda(int meetingId, MeetingAgendaPostDto meetingAgendaPostDto)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{
					var info = await _meetingManager.UpdateMeetingAgenda(meetingAgendaPostDto, meetingId, UserId, Language);
					return Ok(new ApiResponseDto<List<MeetingAgendaListItemDto>>(info));
				}
				return Unauthorized();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpDelete("{meetingId}/meeting-agenda/{agendaId}")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> DeleteMeetingAgenda(int meetingId, int agendaId)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{
					var agendas = await _meetingManager.DeleteMeetingAgenda(agendaId, meetingId, UserId, Language);

					return Ok(new ApiResponseDto<List<MeetingAgendaListItemDto>>(agendas));
				}
				return Unauthorized();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpPut("{meetingId}/meeting-attachment")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> UpdateMeetingAttachment(int meetingId, AttachmentPutDto attachmentPutDto)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{
					var attachments = await _meetingManager.UpdateMeetingAttachment(attachmentPutDto, meetingId,Language);
					return Ok(new ApiResponseDto<List<AttachmentListItemDto>>(attachments));
				}
				return Unauthorized();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpDelete("{meetingId}/meeting-attachment/{attachmentId}")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> DeleteMeetingAttachment(int meetingId, int attachmentId)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{
					var atachments = await _meetingManager.DeleteMeetingAttachment(attachmentId, meetingId,Language);

					return Ok(new ApiResponseDto<List<AttachmentListItemDto>>(atachments));
				}
				return Unauthorized();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpGet("{meetingId}/meeting-attachments")]
		public async Task<IActionResult> GetMeetingAttachment(int meetingId)
		{
			try
			{
				var hasAccess = await _meetingManager.HasMeetingAttachmentAccess(UserId, meetingId);
				if (hasAccess)
				{
					var atachments = await _meetingManager.GetMeetingAttachments(meetingId,UserId,Language);

					return Ok(new ApiResponseDto<List<AttachmentListItemDto>>(atachments));
				}
				return Ok(new ApiResponseDto<List<AttachmentListItemDto>>(new List<AttachmentListItemDto>()));

			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpPost("{meetingId}/meeting-attachment")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> AddMeetingAttachment(int meetingId, [FromQuery] short privacyId ,[FromQuery] int?agendaId)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{
					FileStatusEnum status = FileStatusEnum.Valid;
					IFormCollection formCollection = await Request.ReadFormAsync();
					if (formCollection.Files.Count > 0)
					{
						(status, _) = base.AreValidAttachments(formCollection.Files);

					}
					if (status == FileStatusEnum.Valid)
					{
						var atachments = await _meetingManager.AddMeetingAttachments(meetingId, formCollection.Files,  UserId, privacyId, agendaId, Language);

						var usersIds = await _meetingManager.ListMeetingAttendeesIds(meetingId);
						await _intalioHub.NotifyMeetingAttachmentsChange(usersIds.Select(x => x.ToString()).ToArray(),meetingId, atachments);
					
						return Ok(new ApiResponseDto<List<AttachmentListItemDto>>(atachments));
					}
					else
					{
						return Ok(new ApiResponseDto<bool>(Success: false,Message: status.GetDisplayName()));
					}
				}
				return Unauthorized();
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("{meetingId}/associated-meeting")]
		public async Task<IActionResult> ListAssociatedMeeting(int meetingId)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{
					List<AssociatedMeetingDto> associateds = await _meetingManager.ListAssociatedMeeting(meetingId);
					return Ok(new ApiResponseDto<List<AssociatedMeetingDto>>(associateds));

				}
				return Unauthorized();

			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpDelete("{meetingId}/associated-meeting/{associatedId}")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> DeleteAssociatedMeeting(int meetingId, int associatedId)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{
					var associateds = await _meetingManager.DeleteAssociatedMeeting(meetingId, associatedId);
					return Ok(new ApiResponseDto<List<AssociatedMeetingDto>>(associateds));
				}
				return Unauthorized();
				
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpPost("{meetingId}/associated-meeting/{associatedId}")]
		[RequiredPermission(PermissionDbEnum.CreateMeeting, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> AddAssociatedMeeting(int meetingId, int associatedId)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{
					var associateds = await _meetingManager.AddAssociatedMeeting(meetingId, associatedId);

					return Ok(new ApiResponseDto<List<AssociatedMeetingDto>>(associateds));
				}
				return Unauthorized();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		#endregion

        #region MeetingSummary


		[HttpGet("{meetingId}/meeting-summary")]
		public async Task<IActionResult> GetMeetingSummary(int meetingId)
		{
			try
			{
				var hasAccess = await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccess)
				{
					var meetingSummary = await _meetingManager.GetMeetingSummaryAsync(meetingId);

					return Ok(new ApiResponseDto<string>(Data: meetingSummary, Success: meetingSummary != null));
				}
				return Unauthorized();
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpPost("{meetingId}/meeting-summary")]
		public async Task<IActionResult> AddOrUpdateMeetingSummary(int meetingId,[FromBody] string summary)
		{
			try
			{
				var hasAccessToUpdate =await _meetingManager.IsMeetingOwner(UserId, meetingId);
				if (hasAccessToUpdate) 
				{
					await _meetingManager.SaveMeetingSummaryAsync(UserId, meetingId, summary);
					return Ok(new ApiResponseDto<bool>(Success: true));
				}
				return Unauthorized();

			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}
		#endregion

		[HttpGet("{meetingId}/meeting-agenda-vote-reports")]
		public async Task<IActionResult> ListMeetingAgendaVotingReport(int meetingId)
		{
			try
			{
				var hasAccess = await _meetingManager.HasMeetingAVotingResultsAccess(UserId, meetingId);
				if (hasAccess)
				{
					var votes = await _meetingManager.ListMeetingAgendaVotingReportAsync(meetingId, Language);
					return Ok(new ApiResponseDto<List<MeetingAgendaVotingReportDto>>(votes));
				}
				return Unauthorized();

			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		#region PDF-based Minutes of Meeting

		/// <summary>
		/// Generates a PDF-based Minutes of Meeting document.
		/// This is the new improved method that generates PDFs directly without Word templates.
		/// </summary>
		[HttpPost("{meetingId}/generate-pdf-minutes")]
		public async Task<IActionResult> GeneratePdfMeetingMinutes(int meetingId, [FromBody] GenerateMeetingMinutesRequestDto request)
		{
			try
			{
				var hasAccess = await _meetingManager.CanViewMeetingMinutes(UserId, meetingId);
				if (hasAccess)
				{
					request.MeetingId = meetingId;
					var result = await _meetingManager.GeneratePdfMeetingMinutes(request, UserId, Language);

					if (result.Success)
					{
						// Notify attendees about new minutes
						var usersIds = await _meetingManager.ListMeetingAttendeesIds(meetingId);
						if (!usersIds.Any(x => x == UserId))
						{
							usersIds.Add(UserId);
						}
						await _intalioHub.NotifyMeetingStatusChange(
							usersIds.Select(x => x.ToString()).ToArray(),
							meetingId,
							MeetingStatusDbEnum.PendingInitialMeetingMinutesApproval);
					}

					return Ok(new ApiResponseDto<GenerateMeetingMinutesResponseDto>(result));
				}
				return Unauthorized();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		/// <summary>
		/// Gets meeting minutes data for preview (without saving to storage).
		/// Use this to show a preview before generating the final PDF.
		/// </summary>
		[HttpGet("{meetingId}/minutes-preview")]
		public async Task<IActionResult> GetMeetingMinutesPreview(int meetingId, [FromQuery] bool includePrivateNotes = false)
		{
			try
			{
				var hasAccess = await _meetingManager.CanViewMeetingMinutes(UserId, meetingId);
				if (hasAccess)
				{
					var preview = await _meetingManager.GetMeetingMinutesPreview(meetingId, UserId, Language, includePrivateNotes);
					if (preview != null)
					{
						return Ok(new ApiResponseDto<MeetingMinutesDto>(preview));
					}
					return NotFound(new ApiResponseDto<object>(Success: false, Message: "Meeting not found"));
				}
				return Unauthorized();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		#endregion
	}
}
