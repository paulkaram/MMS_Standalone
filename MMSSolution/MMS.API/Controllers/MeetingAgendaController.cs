using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.BLL.Managers;
using MMS.DTO;
using MMS.API.Common.Hubs;
using MMS.DTO.Meetings;
using MMS.DAL.Models.MMS;

namespace MMS.API.Controllers
{
    [Route("api/meetingAgenda")]
	[ApiController]
	public class MeetingAgendaController : IntalioBaseController
	{
		private readonly MeetingAgendaManager _meetingAgendaManager;
		private readonly IMainHub _intalioHub;
		private readonly MmsNotificationService _notify;
		public MeetingAgendaController(MeetingAgendaManager meetingAgendaManager,
						IMainHub hub, MmsNotificationService notify)
		{
			_meetingAgendaManager = meetingAgendaManager;
			_intalioHub = hub;
			_notify = notify;
		}
		[HttpPost("meeting-agenda-vote")]
		public async Task<IActionResult> AddMeetingAgendaVote(MeetingUserVotePostDto meetingUserVotePostDto)
		{
			try
			{
				bool hasAccess = await _meetingAgendaManager.isMeetingMember(meetingUserVotePostDto.MeetingAgendaId, UserId);
				if (hasAccess)
				{
					var votes = await _meetingAgendaManager.AddMeetingAgendaVote(UserId, meetingUserVotePostDto);
					return Ok(new ApiResponseDto<List<MeetingUserVoteDto>>(votes));
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

		[HttpGet("{meetingAgendaId}/meeting-agenda-vote-results")]
		public async Task<IActionResult> ListMeetingAgendaVotingResults(int meetingAgendaId)
		{
			try
			{
				bool hasAccess = await _meetingAgendaManager.IsMeetingOwner(UserId, meetingAgendaId);
				if (hasAccess)
				{
					var votes = await _meetingAgendaManager.ListMeetingAgendaVotingResultsAsync(meetingAgendaId, Language);
					return Ok(new ApiResponseDto<List<MeetingAgendaVotingResultsListItemDto>>(votes));
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

		#region Agenda Notes

		[HttpPost("note")]
		public async Task<IActionResult> AddMeetingAgendaNote(MeetingAgendaNotePostDto meetingAgendaNotePostDto)
		{
			try
			{
				bool hasAccess = await _meetingAgendaManager.isMeetingMember(meetingAgendaNotePostDto.MeetingAgendaId, UserId);
				if (hasAccess)
				{
					bool added = await _meetingAgendaManager.AddMeetingAgendaNote(UserId, meetingAgendaNotePostDto);

					// Notify via SignalR for public notes
					if (added && meetingAgendaNotePostDto.IsPublic)
					{
						var meetingId = await _meetingAgendaManager.GetMeetingIdByAgendaId(meetingAgendaNotePostDto.MeetingAgendaId);
						if (meetingId.HasValue)
						{
							await _intalioHub.NotifyMeetingAgendaNotesChange(meetingId.Value, meetingAgendaNotePostDto.MeetingAgendaId);
						}
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
				return base.ErrorResponse(ex);
			}
		}
		[HttpGet("{meetingAgendaId}/note")]
		public async Task<IActionResult> ListMeetingAgendaNotes(int meetingAgendaId)
		{
			try
			{
				bool hasAccess = await _meetingAgendaManager.isMeetingMember(meetingAgendaId, UserId);
				if (hasAccess)
				{
					var notes = await _meetingAgendaManager.ListMeetingAgendaNotes(UserId, meetingAgendaId, Language);

					return Ok(new ApiResponseDto<List<MeetingAgendaNoteListItemDto>>(notes));
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
		[HttpPut("note")]
		public async Task<IActionResult> UpdateMeetingAgendaNote(MeetingAgendaNotePutDto meetingAgendaNotePutDto)
		{
			try
			{
				bool hasAccess = await _meetingAgendaManager.canEditNote(meetingAgendaNotePutDto.Id, UserId);
				if (hasAccess)
				{
					// Get note info before update for SignalR notification
					var noteInfo = await _meetingAgendaManager.GetNoteInfoForSignalR(meetingAgendaNotePutDto.Id);
					bool wasPublic = noteInfo.IsPublic;

					bool success = await _meetingAgendaManager.UpdateMeetingAgendaNote(meetingAgendaNotePutDto);

					// Notify via SignalR if note is/was public
					if (success && (meetingAgendaNotePutDto.IsPublic || wasPublic) && noteInfo.MeetingId.HasValue && noteInfo.AgendaId.HasValue)
					{
						await _intalioHub.NotifyMeetingAgendaNotesChange(noteInfo.MeetingId.Value, noteInfo.AgendaId.Value);
					}

					return Ok(new ApiResponseDto<bool>(success));
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
		[HttpDelete("{meetingAgendaNoteId}/note")]
		public async Task<IActionResult> DeleteMeetingAgendaNote(int meetingAgendaNoteId)
		{
			try
			{
				bool hasAccess = await _meetingAgendaManager.canEditNote(meetingAgendaNoteId, UserId);
				if (hasAccess)
				{
					// Get note info before delete for SignalR notification
					var noteInfo = await _meetingAgendaManager.GetNoteInfoForSignalR(meetingAgendaNoteId);

					bool deleted = await _meetingAgendaManager.DeleteMeetingAgendaNote(meetingAgendaNoteId);

					// Notify via SignalR if note was public
					if (deleted && noteInfo.IsPublic && noteInfo.MeetingId.HasValue && noteInfo.AgendaId.HasValue)
					{
						await _intalioHub.NotifyMeetingAgendaNotesChange(noteInfo.MeetingId.Value, noteInfo.AgendaId.Value);
					}

					return Ok(new ApiResponseDto<bool>(deleted));
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
		#endregion

		#region Agenda Recommendations
		[HttpPost("recommendation")]
		public async Task<IActionResult> AddMeetingAgendaRecommendation(MeetingAgendaRecommendationPostDto meetingAgendaRecommendationPostDto)
		{
			try
			{
				bool hasAccessToUpdate = await _meetingAgendaManager.IsMeetingOwner(UserId, meetingAgendaRecommendationPostDto.MeetingAgendaId);
				if (hasAccessToUpdate)
				{
					bool added = await _meetingAgendaManager.AddMeetingAgendaRecommendation(UserId, meetingAgendaRecommendationPostDto);
					return Ok(new ApiResponseDto<bool>(added));
				}
				return Unauthorized();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpGet("{meetingAgendaId}/recommendation")]
		public async Task<IActionResult> ListMeetingAgendaRecommendations(int meetingAgendaId)
		{
			try
			{
				bool hasAccess = await _meetingAgendaManager.IsMeetingOwner(UserId, meetingAgendaId);
				if (hasAccess)
				{
					var Recommendations = await _meetingAgendaManager.ListMeetingAgendaRecommendations(meetingAgendaId, Language);
					return Ok(new ApiResponseDto<List<MeetingAgendaRecommendationListItemDto>>(Recommendations));
				}
				return Unauthorized();


			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpPut("recommendation")]
		public async Task<IActionResult> UpdateMeetingAgendaRecommendation(MeetingAgendaRecommendationPutDto meetingAgendaRecommendationPutDto)
		{
			try
			{
				bool hasAccessToUpdate = await _meetingAgendaManager.canEditRecommendation( meetingAgendaRecommendationPutDto.Id, UserId);
				if (hasAccessToUpdate)
				{
					bool success = await _meetingAgendaManager.UpdateMeetingAgendaRecommendation(meetingAgendaRecommendationPutDto);
					return Ok(new ApiResponseDto<bool>(success));
				}
				return Unauthorized();
				
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		[HttpDelete("{meetingAgendaRecommendationId}/recommendation")]
		public async Task<IActionResult> DeleteMeetingAgendaRecommendation(int meetingAgendaRecommendationId)
		{
			try
			{
				bool hasAccessToUpdate = await _meetingAgendaManager.canEditRecommendation(meetingAgendaRecommendationId,UserId);
				if (hasAccessToUpdate)
				{
					bool deleted = await _meetingAgendaManager.DeleteMeetingAgendaRecommendation(meetingAgendaRecommendationId);
					return Ok(new ApiResponseDto<bool>(deleted));
				}

				return Unauthorized();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}
		#endregion

		#region Agenda Summary

		[HttpGet("{meetingAgendaId}/summary")]
		public async Task<IActionResult> GetMeetingAgendaSummary(int meetingAgendaId)
		{
			try
			{
				bool hasAccess = await _meetingAgendaManager.isMeetingMember(meetingAgendaId, UserId);
				if (hasAccess)
				{
					var summary = await _meetingAgendaManager.GetAgendaSummaryAsync(meetingAgendaId);
					return Ok(new ApiResponseDto<string>(Data: summary, Success: true));
				}
				return Unauthorized();
			}
			catch (Exception ex)
			{
				return base.ErrorResponse(ex);
			}
		}

		[HttpPost("{meetingAgendaId}/summary")]
		public async Task<IActionResult> SaveMeetingAgendaSummary(int meetingAgendaId, [FromBody] string summary)
		{
			try
			{
				bool hasAccess = await _meetingAgendaManager.IsMeetingOwner(UserId, meetingAgendaId);
				if (hasAccess)
				{
					await _meetingAgendaManager.SaveAgendaSummaryAsync(UserId, meetingAgendaId, summary);

					// Notify via SignalR - reuse notes change notification to signal agenda update
					var meetingId = await _meetingAgendaManager.GetMeetingIdByAgendaId(meetingAgendaId);
					if (meetingId.HasValue)
					{
						await _intalioHub.NotifyMeetingAgendaNotesChange(meetingId.Value, meetingAgendaId);
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

		#endregion
	}
}
