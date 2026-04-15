using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Constants;
using MMS.BLL.Managers;
using MMS.DTO;
using MMS.DTO.Workflow;

namespace MMS.API.Controllers
{
    /// <summary>
    /// Runtime workflow endpoints — used by the bid detail page and the
    /// Tasks page. Not permission-gated beyond [Authorize] on the base
    /// controller; each action is scoped to the current user's workflow tasks.
    /// </summary>
    [Route("api/workflow")]
    [ApiController]
    public class WorkflowController : IntalioBaseController
    {
        private readonly WorkflowManager _wfManager;

        public WorkflowController(WorkflowManager wfManager)
        {
            _wfManager = wfManager;
        }

        [HttpGet("bid/{bidId:int}/instance")]
        public async Task<IActionResult> GetInstanceForBid(int bidId)
        {
            try
            {
                var instance = await _wfManager.GetInstanceForBidAsync(bidId, UserId, Language);
                return instance == null ? NotFound() : Ok(new ApiResponseDto<WorkflowInstanceDto>(instance));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpGet("instance/{instanceId:int}/history")]
        public async Task<IActionResult> GetHistory(int instanceId)
        {
            try
            {
                var history = await _wfManager.GetHistoryAsync(instanceId, Language);
                return Ok(new ApiResponseDto<List<WorkflowHistoryItemDto>>(history));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpPost("instance/{instanceId:int}/fire")]
        [LogUserActivity(AuditOperationConstants.Update, "Fired workflow transition on instance {instanceId}")]
        public async Task<IActionResult> FireTransition(int instanceId, [FromBody] FireTransitionDto dto)
        {
            try
            {
                await _wfManager.FireTransitionAsync(instanceId, dto, UserId);
                return Ok(new ApiResponseDto<bool>(true));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpGet("my-tasks")]
        public async Task<IActionResult> MyTasks([FromQuery] bool includeCompleted = false)
        {
            try
            {
                var tasks = await _wfManager.ListMyTasksAsync(UserId, includeCompleted, Language);
                return Ok(new ApiResponseDto<List<WorkflowTaskDto>>(tasks));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }
    }
}
