using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Constants;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DTO.ExternalMembers;

namespace MMS.API.Controllers
{
    [Route("api/external-members")]
    [ApiController]
    public class ExternalMembersController : IntalioBaseController
    {
        private readonly ExternalMemberManager _externalMemberManager;

        public ExternalMembersController(ExternalMemberManager externalMemberManager)
        {
            _externalMemberManager = externalMemberManager;
        }

        [HttpGet("admin")]
        [RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> ListAdmin()
        {
            try
            {
                var members = await _externalMemberManager.ListAdminAsync();
                return Ok(new ApiResponseDto<List<ExternalMemberDto>>(members));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListAutoComplete([FromQuery] string? search = null)
        {
            try
            {
                var members = await _externalMemberManager.ListAutoCompleteAsync(search, Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(members));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPost]
        [RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.Create, "Created external member")]
        public async Task<IActionResult> Create([FromBody] ExternalMemberPostDto dto)
        {
            try
            {
                var member = await _externalMemberManager.CreateAsync(dto, UserId);
                return Ok(new ApiResponseDto<ExternalMemberDto>(member));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPut("{id}")]
        [RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.Update, "Updated external member {id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ExternalMemberPostDto dto)
        {
            try
            {
                var member = await _externalMemberManager.UpdateAsync(id, dto);
                if (member == null)
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));
                return Ok(new ApiResponseDto<ExternalMemberDto>(member));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpDelete("{id}")]
        [RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.Delete, "Deleted external member {id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var removed = await _externalMemberManager.DeleteAsync(id);
                if (!removed)
                    return Conflict(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));
                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("committee/{committeeId}")]
        public async Task<IActionResult> ListByCommittee(int committeeId)
        {
            try
            {
                var members = await _externalMemberManager.ListByCommitteeAsync(committeeId, Language);
                return Ok(new ApiResponseDto<List<CommitteeExternalMemberDto>>(members));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPost("committee")]
        [LogUserActivity(AuditOperationConstants.Create, "Added external member to committee")]
        public async Task<IActionResult> AddToCommittee([FromBody] CommitteeExternalMemberPostDto dto)
        {
            try
            {
                var link = await _externalMemberManager.AddToCommitteeAsync(dto, Language);
                return Ok(new ApiResponseDto<CommitteeExternalMemberDto>(link));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpDelete("committee/{linkId}")]
        [LogUserActivity(AuditOperationConstants.Delete, "Removed external member from committee {linkId}")]
        public async Task<IActionResult> RemoveFromCommittee(int linkId)
        {
            try
            {
                var removed = await _externalMemberManager.RemoveFromCommitteeAsync(linkId);
                if (!removed)
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));
                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
    }
}
