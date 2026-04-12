using Intalio.Tools.Common.Enumerations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Constants;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DTO.Meetings;
using System.Security.Claims;

namespace MMS.API.Controllers
{
    [Route("api/meetingAgendaRecommendations")]
    [ApiController]
    public class MeetingAgendaRecommendationsController : IntalioBaseController
    {
        private readonly MeetingAgendaRecommendationsManager _meetingAgendaRecommendationsManager;
        public MeetingAgendaRecommendationsController(MeetingAgendaRecommendationsManager meetingAgendaManager)
        {
            _meetingAgendaRecommendationsManager = meetingAgendaManager;

        }
        [HttpPost("search/{Page}/{PageSize}")]
		[RequiredPermission(PermissionDbEnum.Recommendations, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> SearchMeetingAgendaRecommendations(SearchRecommendationsDto searchRecommendationsDto, int Page = 1, int PageSize = PaginationConstants.DefaultPageSize)
        {
            try
            {
                (Page, PageSize) = PaginationConstants.ValidatePagination(Page, PageSize);
                var recommendations = await _meetingAgendaRecommendationsManager.ListMeetingAgendaRecommendations(searchRecommendationsDto, UserId, Language, Page,PageSize);
                return Ok(new ApiResponseDto<GenericPaginationListDto<MeetingAgendaRecommendationFollowUpListItemDto>>(recommendations));

            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
        [Authorize("OIDC")]
        [HttpPost("hub-search/{langCode}/{Page}/{PageSize}")]
        public async Task<IActionResult> SearchMeetingAgendaRecommendationsForHub(SearchRecommendationsDto searchRecommendationsDto,string langCode, int Page = 1, int PageSize = PaginationConstants.DefaultPageSize)
        {
            try
            {
                (Page, PageSize) = PaginationConstants.ValidatePagination(Page, PageSize);
                string username = User.FindFirstValue("Username") == null ? "" : User.FindFirstValue("Username");
                string email = User.FindFirstValue("Email") == null ? "" : User.FindFirstValue("Email");
                if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(email))
                {
                    return BadRequest();
                }
                var lang = langCode == "en" ? LanguageDbEnum.English : LanguageDbEnum.Arabic;
                var recommendations = await _meetingAgendaRecommendationsManager.ListMeetingAgendaRecommendationsForHub(email,username,searchRecommendationsDto, lang, Page, PageSize);
                return Ok(new ApiResponseDto<GenericPaginationListDto<MeetingAgendaRecommendationFollowUpListItemDto>>(recommendations));

            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("{RecommendationId}/{Page}/{PageSize}/recommendation-notes")]
		[RequiredPermission(PermissionDbEnum.Recommendations, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetRecommendationNotes(int recommendationId, int Page, int PageSize)
        {
            try
            {
                if (await _meetingAgendaRecommendationsManager.HasViewAccess(UserId, recommendationId))
                {
                    var notes = await _meetingAgendaRecommendationsManager.ListRecommendationNotes(recommendationId, Page, PageSize);
                    return Ok(new ApiResponseDto<GenericPaginationListDto<RecommendationNoteListItemDto>>(notes));
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

        [HttpPost("recommendation-notes")]
		[RequiredPermission(PermissionDbEnum.Recommendations, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> AddRecommendationNotes(RecommendationNoteListItemDto recommendationNoteListItemDto)
        {
            try
            {
                var hasAccess=await _meetingAgendaRecommendationsManager.HasWriteAccess(UserId, recommendationNoteListItemDto.RecommendationId);
                if (hasAccess)
                {
					var added = await _meetingAgendaRecommendationsManager.AddRecommendationNote(recommendationNoteListItemDto);
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

        [HttpPut("recommendation-notes")]
		[RequiredPermission(PermissionDbEnum.Recommendations, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> UpdateRecommendationNotes(RecommendationNoteListItemDto recommendationNoteListItemDto)
        {
            try
            {
				var hasAccess = await _meetingAgendaRecommendationsManager.HasAccessToNote(UserId, recommendationNoteListItemDto.Id);
                if (hasAccess)
                {
					var updated = await _meetingAgendaRecommendationsManager.UpdateRecommendationNote(recommendationNoteListItemDto);
					return Ok(new ApiResponseDto<bool>(updated));
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

        [HttpDelete("{RecommendationNoteId}/recommendation-notes")]
		[RequiredPermission(PermissionDbEnum.Recommendations, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> DeleteRecommendationNotes(int RecommendationNoteId)
        {
            try
            {
				var hasAccess = await _meetingAgendaRecommendationsManager.HasAccessToNote(UserId, RecommendationNoteId);
                if (hasAccess)
                {
					var removed = await _meetingAgendaRecommendationsManager.DeleteRecommendationNote(RecommendationNoteId);
					return Ok(new ApiResponseDto<bool>(removed));
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

        [HttpGet("{recommendationId}/{Page}/{PageSize}/recommendation-attachments")]
		[RequiredPermission(PermissionDbEnum.Recommendations, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetRecommendationAttachments(int recommendationId, int Page, int PageSize)
        {
            try
            {
                if (await _meetingAgendaRecommendationsManager.HasViewAccess(UserId, recommendationId))
                {
                    var attachments = await _meetingAgendaRecommendationsManager.ListRecommendationAttachments(recommendationId, Page, PageSize);
                    return Ok(new ApiResponseDto<GenericPaginationListDto<AttachmentListItemDto>>(attachments));
                }
                else
                {
                    return Unauthorized(false);
                }
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPost("recommendation-attachments")]
		[RequiredPermission(PermissionDbEnum.Recommendations, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> AddRecommendationAttachment([FromForm] int recommendationId)
        {
            try
            {
				var hasAccess = await _meetingAgendaRecommendationsManager.HasWriteAccess(UserId, recommendationId);
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
						await _meetingAgendaRecommendationsManager.AddRecommendationAttachments(recommendationId, formCollection.Files, UserId);

					}
					else
					{
						return Ok(new ApiResponseDto<bool>(Success: false,Message: status.GetDisplayName()));
					}
					return Ok(new ApiResponseDto<bool>(true));
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
		[HttpPost("recommendation-attachments-mobile")]
		[RequiredPermission(PermissionDbEnum.Recommendations, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> AddRecommendationAttachment(RecommendationAttachmentPostDto attachmentDto)
		{
			try
			{
				FileStatusEnum status = FileStatusEnum.Valid;
				FormFile? formFile = null;

				if (!string.IsNullOrEmpty(attachmentDto.FileContent))
				{
					byte[] fileBytes = Convert.FromBase64String(attachmentDto.FileContent);

					formFile = new FormFile(new MemoryStream(fileBytes), 0, fileBytes.Length, "file", attachmentDto.FileName);

					(status, _) = base.AreValidAttachments(new FormFileCollection { formFile });
				}

				if (status == FileStatusEnum.Valid)
				{
					await _meetingAgendaRecommendationsManager.AddRecommendationAttachments(attachmentDto.RecommendationId, new FormFileCollection { formFile }, UserId);
				}
				else
				{
					return Ok(new ApiResponseDto<bool>(Success: false));
				}

				return Ok(new ApiResponseDto<bool>(true));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpDelete("{RecommendationAttachmentId}/recommendation-attachments")]
		[RequiredPermission(PermissionDbEnum.Recommendations, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> DeleteRecommendationAttachment(int RecommendationAttachmentId)
        {
            try
            {
				var hasAccess = await _meetingAgendaRecommendationsManager.HasAccessToAttachment(UserId, RecommendationAttachmentId);
                if (hasAccess)
                {
                    var removed = await _meetingAgendaRecommendationsManager.DeleteRecommendationAttachment(RecommendationAttachmentId);
                    return Ok(new ApiResponseDto<bool>(removed));
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

        [HttpPut]
		[RequiredPermission(PermissionDbEnum.Recommendations, PermissionLevelDbEnum.Write)]
		public async Task<IActionResult> UpdateRecommendation(UpdateRecommendationProgressDto updateRecommendationProgressDto)
        {
            try
            {
				var hasAccess = await _meetingAgendaRecommendationsManager.HasWriteAccess(UserId, updateRecommendationProgressDto.Id);
                if (hasAccess)
                {
					var updated = await _meetingAgendaRecommendationsManager.UpdateRecommendationProgress(updateRecommendationProgressDto);
					return Ok(new ApiResponseDto<bool>(updated));
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

        [HttpGet("{recommendationId}")]
		[RequiredPermission(PermissionDbEnum.Recommendations, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetRecommendation(int recommendationId)
        {
            try
            {
                if (await _meetingAgendaRecommendationsManager.HasViewAccess(UserId, recommendationId))
                {
                    var recommendation = await _meetingAgendaRecommendationsManager.GetRecommendation(recommendationId, Language);
                    return Ok(new ApiResponseDto<MeetingAgendaRecommendationFollowUpListItemDto>(recommendation));
                }
                else

                {
                    return Unauthorized(false);
                }
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

    }
}
