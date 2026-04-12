using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Constants;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DTO.CommitteeItems;

namespace MMS.API.Controllers
{
    [Route("api/committee-items")]
    [ApiController]
    public class CommitteeItemsController : IntalioBaseController
    {
        private readonly CommitteeItemManager _committeeItemManager;

        public CommitteeItemsController(CommitteeItemManager committeeItemManager)
        {
            _committeeItemManager = committeeItemManager;
        }

        [HttpPost]
        [LogUserActivity(AuditOperationConstants.Create, "Created committee item")]
        public async Task<IActionResult> Create(CommitteeItemPostDto dto)
        {
            try
            {
                var item = await _committeeItemManager.CreateAsync(dto, UserId, Language);
                return Ok(new ApiResponseDto<CommitteeItemDto>(item));
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
                var item = await _committeeItemManager.GetAsync(id, Language);
                if (item == null)
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));

                return Ok(new ApiResponseDto<CommitteeItemDto>(item));
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
                var items = await _committeeItemManager.ListByCommitteeAsync(committeeId, Language);
                return Ok(new ApiResponseDto<List<CommitteeItemDto>>(items));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPut("{id}")]
        [LogUserActivity(AuditOperationConstants.Update, "Updated committee item {id}")]
        public async Task<IActionResult> Update(int id, CommitteeItemPostDto dto)
        {
            try
            {
                var item = await _committeeItemManager.UpdateAsync(id, dto, Language);
                if (item == null)
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));

                return Ok(new ApiResponseDto<CommitteeItemDto>(item));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpDelete("{id}")]
        [LogUserActivity(AuditOperationConstants.Delete, "Deleted committee item {id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var removed = await _committeeItemManager.DeleteAsync(id);
                if (!removed)
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));

                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("types")]
        public async Task<IActionResult> ListItemTypes()
        {
            try
            {
                var types = await _committeeItemManager.ListItemTypesAsync(Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(types));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPost("types")]
        [RequiredPermission(PermissionDbEnum.Lookups, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.Create, "Created item type")]
        public async Task<IActionResult> CreateItemType([FromBody] CommitteeItemTypePostDto dto)
        {
            try
            {
                var type = await _committeeItemManager.CreateItemTypeAsync(dto);
                return Ok(new ApiResponseDto<CommitteeItemTypeDto>(type));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPut("types/{id}")]
        [RequiredPermission(PermissionDbEnum.Lookups, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.Update, "Updated item type {id}")]
        public async Task<IActionResult> UpdateItemType(int id, [FromBody] CommitteeItemTypePostDto dto)
        {
            try
            {
                var type = await _committeeItemManager.UpdateItemTypeAsync(id, dto);
                if (type == null)
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));

                return Ok(new ApiResponseDto<CommitteeItemTypeDto>(type));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpDelete("types/{id}")]
        [RequiredPermission(PermissionDbEnum.Lookups, PermissionLevelDbEnum.Write)]
        [LogUserActivity(AuditOperationConstants.Delete, "Deleted item type {id}")]
        public async Task<IActionResult> DeleteItemType(int id)
        {
            try
            {
                var removed = await _committeeItemManager.DeleteItemTypeAsync(id);
                if (!removed)
                    return Conflict(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));

                return Ok(new ApiResponseDto<object>(Success: true));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("types/admin")]
        [RequiredPermission(PermissionDbEnum.Lookups, PermissionLevelDbEnum.Read)]
        public async Task<IActionResult> ListItemTypesAdmin()
        {
            try
            {
                var types = await _committeeItemManager.ListItemTypesAdminAsync();
                return Ok(new ApiResponseDto<List<CommitteeItemTypeDto>>(types));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
    }
}
