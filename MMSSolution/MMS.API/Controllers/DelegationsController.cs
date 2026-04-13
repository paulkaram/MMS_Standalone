using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Constants;
using MMS.BLL.Managers;
using MMS.DTO;
using MMS.DTO.Delegations;

namespace MMS.API.Controllers
{
    [Route("api/delegations")]
    [ApiController]
    public class DelegationsController : IntalioBaseController
    {
        private readonly DelegationManager _delegationManager;

        public DelegationsController(DelegationManager delegationManager)
        {
            _delegationManager = delegationManager;
        }

        [HttpGet("outgoing")]
        public async Task<IActionResult> ListOutgoing()
        {
            try
            {
                var items = await _delegationManager.ListOutgoingAsync(UserId, Language);
                return Ok(new ApiResponseDto<List<DelegationDto>>(items));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("incoming")]
        public async Task<IActionResult> ListIncoming()
        {
            try
            {
                var items = await _delegationManager.ListIncomingAsync(UserId, Language);
                return Ok(new ApiResponseDto<List<DelegationDto>>(items));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var d = await _delegationManager.GetAsync(id, Language);
                if (d == null)
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));
                return Ok(new ApiResponseDto<DelegationDto>(d));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPost]
        [LogUserActivity(AuditOperationConstants.Create, "Created delegation")]
        public async Task<IActionResult> Create([FromBody] DelegationPostDto dto)
        {
            try
            {
                var delegation = await _delegationManager.CreateAsync(UserId, dto, Language);
                return Ok(new ApiResponseDto<DelegationDto>(delegation));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPost("{id}/revoke")]
        [LogUserActivity(AuditOperationConstants.Update, "Revoked delegation {id}")]
        public async Task<IActionResult> Revoke(int id)
        {
            try
            {
                var d = await _delegationManager.RevokeAsync(id, UserId, Language);
                if (d == null)
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));
                return Ok(new ApiResponseDto<DelegationDto>(d));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpDelete("{id}")]
        [LogUserActivity(AuditOperationConstants.Delete, "Deleted delegation {id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var removed = await _delegationManager.DeleteAsync(id, UserId);
                if (!removed)
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));
                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
    }
}
