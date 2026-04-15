using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Constants;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DTO.Procurement;

namespace MMS.API.Controllers
{
    /// <summary>
    /// Procurement committees (§5.11) — Opening / Examination / Qualification.
    /// Projects are the top-level entity; competitors + their typed attachments
    /// hang off each project. ERP lookup is stubbed until task #29.
    /// </summary>
    [Route("api/procurement")]
    [ApiController]
    public class ProcurementController : IntalioBaseController
    {
        private readonly ProcurementManager _procurement;

        public ProcurementController(ProcurementManager procurement)
        {
            _procurement = procurement;
        }

        // ─────── ERP lookup (stub) ───────
        [HttpGet("erp/lookup/{poNumber}")]
        public async Task<IActionResult> LookupErpProject(string poNumber)
        {
            try
            {
                var result = await _procurement.LookupErpProjectAsync(poNumber);
                return Ok(new ApiResponseDto<ErpProjectLookupDto>(result));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        // ─────── Projects ───────
        [HttpGet("projects")]
        public async Task<IActionResult> ListProjects()
        {
            try
            {
                var list = await _procurement.ListProjectsAsync(Language);
                return Ok(new ApiResponseDto<List<ProcurementProjectDto>>(list));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpGet("projects/{id:int}")]
        public async Task<IActionResult> GetProject(int id)
        {
            try
            {
                var p = await _procurement.GetProjectAsync(id, Language);
                return p == null ? NotFound() : Ok(new ApiResponseDto<ProcurementProjectDto>(p));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpPost("projects")]
        [LogUserActivity(AuditOperationConstants.Create, "Created procurement project")]
        public async Task<IActionResult> CreateProject([FromBody] ProcurementProjectPostDto dto)
        {
            try
            {
                var created = await _procurement.CreateProjectAsync(dto, UserId, Language);
                return Ok(new ApiResponseDto<ProcurementProjectDto>(created));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpPut("projects/{id:int}")]
        [LogUserActivity(AuditOperationConstants.Update, "Updated procurement project {id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] ProcurementProjectPostDto dto)
        {
            try
            {
                var updated = await _procurement.UpdateProjectAsync(id, dto, UserId, Language);
                return updated == null ? NotFound() : Ok(new ApiResponseDto<ProcurementProjectDto>(updated));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpDelete("projects/{id:int}")]
        [LogUserActivity(AuditOperationConstants.Delete, "Deleted procurement project {id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                var ok = await _procurement.DeleteProjectAsync(id, UserId);
                return Ok(new ApiResponseDto<bool>(ok));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        // ─────── Competitors ───────
        [HttpGet("projects/{projectId:int}/competitors")]
        public async Task<IActionResult> ListCompetitors(int projectId)
        {
            try
            {
                var list = await _procurement.ListCompetitorsAsync(projectId, Language);
                return Ok(new ApiResponseDto<List<CompetitorDto>>(list));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpPost("projects/{projectId:int}/competitors")]
        [LogUserActivity(AuditOperationConstants.Create, "Added competitor to project {projectId}")]
        public async Task<IActionResult> AddCompetitor(int projectId, [FromBody] CompetitorPostDto dto)
        {
            try
            {
                var added = await _procurement.AddCompetitorAsync(projectId, dto, Language);
                return Ok(new ApiResponseDto<CompetitorDto>(added));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpPut("competitors/{competitorId:int}")]
        [LogUserActivity(AuditOperationConstants.Update, "Updated competitor {competitorId}")]
        public async Task<IActionResult> UpdateCompetitor(int competitorId, [FromBody] CompetitorPostDto dto)
        {
            try
            {
                var updated = await _procurement.UpdateCompetitorAsync(competitorId, dto, Language);
                return updated == null ? NotFound() : Ok(new ApiResponseDto<CompetitorDto>(updated));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpDelete("competitors/{competitorId:int}")]
        [LogUserActivity(AuditOperationConstants.Delete, "Removed competitor {competitorId}")]
        public async Task<IActionResult> RemoveCompetitor(int competitorId)
        {
            try
            {
                var ok = await _procurement.RemoveCompetitorAsync(competitorId);
                return Ok(new ApiResponseDto<bool>(ok));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }
    }
}
