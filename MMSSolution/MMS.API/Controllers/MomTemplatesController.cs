using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DTO.Settings;

namespace MMS.API.Controllers
{
    [Authorize]
    [Route("api/mom-templates")]
    public class MomTemplatesController : IntalioBaseController
    {
        private readonly MomTemplateManager _momTemplateManager;

        public MomTemplatesController(MomTemplateManager momTemplateManager)
        {
            _momTemplateManager = momTemplateManager;
        }

        /// <summary>
        /// Get all MOM templates, optionally filtered by branch
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int? branchId)
        {
            try
            {
                var templates = await _momTemplateManager.ListAsync(branchId);
                return Ok(new ApiResponseDto<List<MomTemplateListItemDto>>(templates));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// Get a specific MOM template by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var template = await _momTemplateManager.GetByIdAsync(id);
                if (template == null)
                {
                    return Ok(new ApiResponseDto<MomTemplateDto?>(null, Success: false, Message: "Template not found"));
                }
                return Ok(new ApiResponseDto<MomTemplateDto>(template));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// Get the default template for a branch and template type
        /// </summary>
        [HttpGet("default")]
        public async Task<IActionResult> GetDefault([FromQuery] int? branchId, [FromQuery] int templateType)
        {
            try
            {
                if (!Enum.IsDefined(typeof(MomTemplateTypeDbEnum), templateType))
                {
                    return Ok(new ApiResponseDto<MomTemplateDto?>(null, Success: false, Message: "Invalid template type"));
                }

                var template = await _momTemplateManager.GetDefaultTemplateAsync(branchId, (MomTemplateTypeDbEnum)templateType);
                if (template == null)
                {
                    return Ok(new ApiResponseDto<MomTemplateDto?>(null, Success: false, Message: "No default template found"));
                }
                return Ok(new ApiResponseDto<MomTemplateDto>(template));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// Create a new MOM template
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MomTemplateCreateDto dto)
        {
            try
            {
                if (!Enum.IsDefined(typeof(MomTemplateTypeDbEnum), dto.TemplateType))
                {
                    return Ok(new ApiResponseDto<int>(0, Success: false, Message: "Invalid template type"));
                }

                var id = await _momTemplateManager.CreateAsync(dto, UserId);
                return Ok(new ApiResponseDto<int>(id, Success: true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// Update an existing MOM template
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MomTemplateUpdateDto dto)
        {
            try
            {
                if (id != dto.Id)
                {
                    return Ok(new ApiResponseDto<bool>(false, Success: false, Message: "ID mismatch"));
                }

                if (!Enum.IsDefined(typeof(MomTemplateTypeDbEnum), dto.TemplateType))
                {
                    return Ok(new ApiResponseDto<bool>(false, Success: false, Message: "Invalid template type"));
                }

                var updated = await _momTemplateManager.UpdateAsync(dto, UserId);
                return Ok(new ApiResponseDto<bool>(updated, Success: updated, Message: updated ? null : "Failed to update template"));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// Delete a MOM template (cannot delete system templates)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _momTemplateManager.DeleteAsync(id);
                return Ok(new ApiResponseDto<bool>(deleted, Success: deleted,
                    Message: deleted ? null : "Cannot delete system template or template not found"));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// Get all available template types
        /// </summary>
        [HttpGet("template-types")]
        public async Task<IActionResult> GetTemplateTypes()
        {
            try
            {
                var types = await _momTemplateManager.GetTemplateTypesAsync();
                return Ok(new ApiResponseDto<List<MomTemplateTypeDto>>(types));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        /// <summary>
        /// Get all branches for dropdown
        /// </summary>
        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                var branches = await _momTemplateManager.GetBranchesAsync();
                return Ok(new ApiResponseDto<List<BranchListItemDto>>(branches));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
    }
}
