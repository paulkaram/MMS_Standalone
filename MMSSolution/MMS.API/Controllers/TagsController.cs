using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Constants;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DTO.Tags;

namespace MMS.API.Controllers
{
    [Route("api/tags")]
    [ApiController]
    public class TagsController : IntalioBaseController
    {
        private readonly TagManager _tagManager;

        public TagsController(TagManager tagManager)
        {
            _tagManager = tagManager;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var tags = await _tagManager.ListAsync(Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(tags));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("picker")]
        public async Task<IActionResult> ListForPicker()
        {
            try
            {
                var tags = await _tagManager.ListForPickerAsync(Language);
                return Ok(new ApiResponseDto<List<TagPickerDto>>(tags));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("admin")]
        [RequiredPermission(PermissionDbEnum.Lookups, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> ListAdmin()
        {
            try
            {
                var tags = await _tagManager.ListAdminAsync();
                return Ok(new ApiResponseDto<List<TagDto>>(tags));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPost]
        [RequiredPermission(PermissionDbEnum.Lookups, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.Create, "Created tag")]
        public async Task<IActionResult> Create([FromBody] TagPostDto dto)
        {
            try
            {
                var tag = await _tagManager.CreateAsync(dto);
                return Ok(new ApiResponseDto<TagDto>(tag));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPut("{id:int}")]
        [RequiredPermission(PermissionDbEnum.Lookups, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.Update, "Updated tag {id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TagPostDto dto)
        {
            try
            {
                var tag = await _tagManager.UpdateAsync(id, dto);
                if (tag == null)
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));
                return Ok(new ApiResponseDto<TagDto>(tag));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpDelete("{id:int}")]
        [RequiredPermission(PermissionDbEnum.Lookups, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.Delete, "Deleted tag {id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var removed = await _tagManager.DeleteAsync(id);
                if (!removed)
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));
                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("entity/{entityType}/{entityId}")]
        public async Task<IActionResult> ListForEntity(int entityType, int entityId)
        {
            try
            {
                var tags = await _tagManager.ListForEntityAsync((TagEntityTypeDbEnum)entityType, entityId, Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(tags));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPost("entity/set")]
        [LogUserActivity(AuditOperationConstants.Update, "Updated tags for entity")]
        public async Task<IActionResult> SetTagsForEntity([FromBody] TagLinkPostDto dto)
        {
            try
            {
                await _tagManager.SetTagsForEntityAsync((TagEntityTypeDbEnum)dto.EntityTypeId, dto.EntityId, dto.TagIds);
                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
    }
}
