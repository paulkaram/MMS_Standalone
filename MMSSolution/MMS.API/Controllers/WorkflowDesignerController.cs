using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DTO.Workflow;

namespace MMS.API.Controllers
{
    /// <summary>
    /// Admin CRUD for workflow templates / steps / transitions.
    /// Gated by ManageOrganization.Write (system admin); plus the actor
    /// options endpoint so the designer can populate its dropdowns.
    /// </summary>
    [Route("api/workflow-designer")]
    [ApiController]
    [RequiredPermission(PermissionDbEnum.ManageOrganization, PermissionLevelDbEnum.Write)]
    public class WorkflowDesignerController : IntalioBaseController
    {
        private readonly WorkflowManager _wfManager;

        public WorkflowDesignerController(WorkflowManager wfManager)
        {
            _wfManager = wfManager;
        }

        // ─────── Actor options (for dropdowns) ───────
        [HttpGet("actor-options")]
        public async Task<IActionResult> GetActorOptions()
        {
            try { return Ok(new ApiResponseDto<ActorOptionsDto>(await _wfManager.GetActorOptionsAsync())); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        // ─────── Templates ───────
        [HttpGet("templates")]
        public async Task<IActionResult> ListTemplates()
        {
            try { return Ok(new ApiResponseDto<List<WorkflowTemplateListItemDto>>(await _wfManager.ListTemplatesAsync(Language))); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpGet("templates/{id:int}")]
        public async Task<IActionResult> GetTemplate(int id)
        {
            try
            {
                var t = await _wfManager.GetTemplateAsync(id, Language);
                return t == null ? NotFound() : Ok(new ApiResponseDto<WorkflowTemplateDto>(t));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpPost("templates")]
        public async Task<IActionResult> CreateTemplate([FromBody] WorkflowTemplatePostDto dto)
        {
            try { return Ok(new ApiResponseDto<WorkflowTemplateDto>(await _wfManager.CreateTemplateAsync(dto, UserId, Language))); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpPut("templates/{id:int}")]
        public async Task<IActionResult> UpdateTemplate(int id, [FromBody] WorkflowTemplatePostDto dto)
        {
            try
            {
                var t = await _wfManager.UpdateTemplateAsync(id, dto, UserId, Language);
                return t == null ? NotFound() : Ok(new ApiResponseDto<WorkflowTemplateDto>(t));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpDelete("templates/{id:int}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            try
            {
                var ok = await _wfManager.DeleteTemplateAsync(id);
                return Ok(new ApiResponseDto<bool>(ok));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        // ─────── Steps ───────
        [HttpPost("templates/{templateId:int}/steps")]
        public async Task<IActionResult> AddStep(int templateId, [FromBody] WorkflowStepPostDto dto)
        {
            try { return Ok(new ApiResponseDto<WorkflowStepDto>(await _wfManager.AddStepAsync(templateId, dto, Language))); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpPut("steps/{stepId:int}")]
        public async Task<IActionResult> UpdateStep(int stepId, [FromBody] WorkflowStepPostDto dto)
        {
            try
            {
                var s = await _wfManager.UpdateStepAsync(stepId, dto, Language);
                return s == null ? NotFound() : Ok(new ApiResponseDto<WorkflowStepDto>(s));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpDelete("steps/{stepId:int}")]
        public async Task<IActionResult> DeleteStep(int stepId)
        {
            try { return Ok(new ApiResponseDto<bool>(await _wfManager.DeleteStepAsync(stepId))); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        // ─────── Transitions ───────
        [HttpPost("templates/{templateId:int}/transitions")]
        public async Task<IActionResult> AddTransition(int templateId, [FromBody] WorkflowTransitionPostDto dto)
        {
            try { return Ok(new ApiResponseDto<WorkflowTransitionDto>(await _wfManager.AddTransitionAsync(templateId, dto, Language))); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpPut("transitions/{transitionId:int}")]
        public async Task<IActionResult> UpdateTransition(int transitionId, [FromBody] WorkflowTransitionPostDto dto)
        {
            try
            {
                var t = await _wfManager.UpdateTransitionAsync(transitionId, dto, Language);
                return t == null ? NotFound() : Ok(new ApiResponseDto<WorkflowTransitionDto>(t));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpDelete("transitions/{transitionId:int}")]
        public async Task<IActionResult> DeleteTransition(int transitionId)
        {
            try { return Ok(new ApiResponseDto<bool>(await _wfManager.DeleteTransitionAsync(transitionId))); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }
    }
}
