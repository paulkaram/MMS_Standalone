using Intalio.Tools.Common.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Constants;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DTO.Sessions;

namespace MMS.API.Controllers
{
    [Route("api/sessions")]
    [ApiController]
    public class SessionsController : IntalioBaseController
    {
        private readonly SessionManager _sessionManager;

        public SessionsController(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        [HttpPost]
        [LogUserActivity(AuditOperationConstants.Create, "Created new session")]
        public async Task<IActionResult> CreateSession(SessionPostDto sessionPostDto)
        {
            try
            {
                var session = await _sessionManager.CreateSessionAsync(sessionPostDto, UserId, Language);
                return Ok(new ApiResponseDto<SessionDto>(session));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSession(int id)
        {
            try
            {
                var session = await _sessionManager.GetSessionAsync(id, Language);
                if (session == null)
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: "Session not found"));

                return Ok(new ApiResponseDto<SessionDto>(session));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("committees")]
        public async Task<IActionResult> ListPresentationRelatedCommittees()
        {
            try
            {
                var committees = await _sessionManager.ListPresentationRelatedCommitteesAsync(UserId, Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(committees));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("item-types")]
        public async Task<IActionResult> ListSessionItemTypes()
        {
            try
            {
                var types = await _sessionManager.ListSessionItemTypesAsync(Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(types));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("search-items")]
        public async Task<IActionResult> SearchSessionItems([FromQuery] string search)
        {
            try
            {
                var items = await _sessionManager.SearchSessionItemsAsync(search, Language);
                return Ok(new ApiResponseDto<List<SessionItemDto>>(items));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpPost("{id}/attachments")]
        [LogUserActivity(AuditOperationConstants.Upload, "Uploaded attachment to session {id}")]
        public async Task<IActionResult> AddAttachments(int id, [FromQuery] short privacyId)
        {
            try
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

                var attachments = await _sessionManager.AddSessionAttachmentsAsync(id, formCollection.Files, UserId, privacyId, Language);
                return Ok(new ApiResponseDto<List<AttachmentListItemDto>>(attachments));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpDelete("{id}/attachments/{attachmentId}")]
        [LogUserActivity(AuditOperationConstants.Delete, "Deleted attachment {attachmentId} from session {id}")]
        public async Task<IActionResult> DeleteAttachment(int id, int attachmentId)
        {
            try
            {
                await _sessionManager.DeleteSessionAttachmentAsync(id, attachmentId);
                var attachments = await _sessionManager.ListSessionAttachmentsAsync(id, Language);
                return Ok(new ApiResponseDto<List<AttachmentListItemDto>>(attachments));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("{id}/attachments/{attachmentId}/download")]
        public async Task<IActionResult> DownloadAttachment(int id, int attachmentId)
        {
            try
            {
                var (filename, bytes) = await _sessionManager.DownloadSessionAttachmentAsync(id, attachmentId);
                if (filename == null || bytes == null)
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: "Attachment not found"));

                return base.FileBytes(filename, bytes);
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }

        [HttpGet("{id}/attachments")]
        public async Task<IActionResult> ListAttachments(int id)
        {
            try
            {
                var attachments = await _sessionManager.ListSessionAttachmentsAsync(id, Language);
                return Ok(new ApiResponseDto<List<AttachmentListItemDto>>(attachments));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }
    }
}
