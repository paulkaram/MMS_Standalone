using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Constants;
using MMS.BLL.Managers;
using MMS.DTO;
using MMS.DTO.Bids;

namespace MMS.API.Controllers
{
    [Route("api/bids")]
    [ApiController]
    public class BidsController : IntalioBaseController
    {
        private readonly BidManager _bidManager;

        public BidsController(BidManager bidManager)
        {
            _bidManager = bidManager;
        }

        [HttpGet("statuses")]
        public async Task<IActionResult> ListStatuses()
        {
            try
            {
                var statuses = await _bidManager.ListStatusesAsync();
                return Ok(new ApiResponseDto<List<BidStatusDto>>(statuses));
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
                var bids = await _bidManager.ListByCommitteeAsync(committeeId, Language);
                return Ok(new ApiResponseDto<List<BidDto>>(bids));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> ListMine()
        {
            try
            {
                var bids = await _bidManager.ListForUserAsync(UserId, Language);
                return Ok(new ApiResponseDto<List<BidDto>>(bids));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var bid = await _bidManager.GetAsync(id, Language);
                if (bid == null)
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));
                return Ok(new ApiResponseDto<BidDetailDto>(bid));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPost]
        [LogUserActivity(AuditOperationConstants.Create, "Created bid")]
        public async Task<IActionResult> Create([FromBody] BidPostDto dto)
        {
            try
            {
                var bid = await _bidManager.CreateAsync(dto, UserId, Language);
                return Ok(new ApiResponseDto<BidDto>(bid));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPut("{id:int}")]
        [LogUserActivity(AuditOperationConstants.Update, "Updated bid {id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BidPostDto dto)
        {
            try
            {
                var bid = await _bidManager.UpdateAsync(id, dto, Language);
                if (bid == null)
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));
                return Ok(new ApiResponseDto<BidDto>(bid));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpDelete("{id:int}")]
        [LogUserActivity(AuditOperationConstants.Delete, "Deleted bid {id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var removed = await _bidManager.DeleteAsync(id, UserId);
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

        /// <summary>
        /// Lightweight member picker for the Add-Bid modal.
        /// Narrows team-leader / stakeholder choices to users who actually
        /// belong to the target committee (rather than every user in the system).
        /// </summary>
        [HttpGet("committee/{committeeId:int}/member-picker")]
        public async Task<IActionResult> ListCommitteeMembersForPicker(int committeeId)
        {
            try
            {
                var members = await _bidManager.ListCommitteeMembersForPickerAsync(committeeId, Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(members));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        // ─────── Description (inline edit on BidDetail) ───────
        [HttpPatch("{id:int}/description")]
        [LogUserActivity(AuditOperationConstants.Update, "Updated description of bid {id}")]
        public async Task<IActionResult> UpdateDescription(int id, [FromBody] BidDescriptionPatchDto dto)
        {
            try
            {
                var ok = await _bidManager.UpdateDescriptionAsync(id, dto.Description, UserId);
                return Ok(new ApiResponseDto<bool>(ok));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        public class BidDescriptionPatchDto { public string? Description { get; set; } }

        // ─────── Attachments (§5.6 — bid = attachments + items) ───────

        [HttpGet("{id:int}/attachments")]
        public async Task<IActionResult> ListAttachments(int id)
        {
            try
            {
                var list = await _bidManager.ListAttachmentsAsync(id, Language);
                return Ok(new ApiResponseDto<List<AttachmentListItemDto>>(list));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpPost("{id:int}/attachments")]
        [LogUserActivity(AuditOperationConstants.Create, "Uploaded attachment(s) to bid {id}")]
        public async Task<IActionResult> UploadAttachments(int id, [FromQuery] short privacyId = 1)
        {
            try
            {
                if (Request.Form.Files.Count == 0)
                    return BadRequest(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));

                var list = await _bidManager.AddAttachmentsAsync(id, Request.Form.Files, UserId, privacyId, Language);
                return Ok(new ApiResponseDto<List<AttachmentListItemDto>>(list));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpDelete("attachments/{attachmentId:int}")]
        [LogUserActivity(AuditOperationConstants.Delete, "Deleted bid attachment {attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(int attachmentId)
        {
            try
            {
                var ok = await _bidManager.RemoveAttachmentAsync(attachmentId, UserId);
                return Ok(new ApiResponseDto<bool>(ok));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        // ─────── Stakeholders (§5.6) ───────

        [HttpGet("{id:int}/stakeholders")]
        public async Task<IActionResult> ListStakeholders(int id)
        {
            try
            {
                var list = await _bidManager.ListStakeholdersAsync(id, Language);
                return Ok(new ApiResponseDto<List<BidStakeholderDto>>(list));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpPost("{id:int}/stakeholders")]
        [LogUserActivity(AuditOperationConstants.Create, "Added stakeholder to bid {id}")]
        public async Task<IActionResult> AddStakeholder(int id, [FromBody] BidStakeholderPostDto dto)
        {
            try
            {
                var added = await _bidManager.AddStakeholderAsync(id, dto, UserId, Language);
                return Ok(new ApiResponseDto<BidStakeholderDto>(added));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpDelete("stakeholders/{stakeholderId:int}")]
        [LogUserActivity(AuditOperationConstants.Delete, "Removed stakeholder {stakeholderId}")]
        public async Task<IActionResult> RemoveStakeholder(int stakeholderId)
        {
            try
            {
                var ok = await _bidManager.RemoveStakeholderAsync(stakeholderId, UserId);
                return Ok(new ApiResponseDto<bool>(ok));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpGet("item-types")]
        public async Task<IActionResult> ListItemTypes()
        {
            try { return Ok(new ApiResponseDto<List<ListItemDto>>(await _bidManager.ListItemTypesAsync(Language))); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpGet("{id:int}/relatable-items")]
        public async Task<IActionResult> ListRelatableItems(int id)
        {
            try { return Ok(new ApiResponseDto<List<ListItemDto>>(await _bidManager.ListRelatableItemsAsync(id, Language))); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpGet("committee/{committeeId:int}/external-members-picker")]
        public async Task<IActionResult> ListExternalMembersForPicker(int committeeId)
        {
            try
            {
                var list = await _bidManager.ListExternalMembersForPickerAsync(committeeId, Language);
                return Ok(new ApiResponseDto<List<ListItemDto>>(list));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        // ─────── Bid items (type-less per §5.6 / §5.7 separation) ───────

        [HttpPost("{id:int}/items")]
        [LogUserActivity(AuditOperationConstants.Create, "Added item to bid {id}")]
        public async Task<IActionResult> AddItem(int id, [FromBody] BidItemPostDto dto)
        {
            try
            {
                var item = await _bidManager.AddItemAsync(id, dto, UserId, Language);
                return Ok(new ApiResponseDto<MMS.DTO.CommitteeItems.CommitteeItemDto>(item));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpPut("items/{itemId:int}")]
        [LogUserActivity(AuditOperationConstants.Update, "Updated bid item {itemId}")]
        public async Task<IActionResult> UpdateItem(int itemId, [FromBody] BidItemPostDto dto)
        {
            try
            {
                var item = await _bidManager.UpdateItemAsync(itemId, dto, UserId, Language);
                return Ok(new ApiResponseDto<MMS.DTO.CommitteeItems.CommitteeItemDto>(item));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpDelete("items/{itemId:int}")]
        [LogUserActivity(AuditOperationConstants.Delete, "Deleted bid item {itemId}")]
        public async Task<IActionResult> DeleteItem(int itemId)
        {
            try
            {
                var ok = await _bidManager.DeleteItemAsync(itemId, UserId);
                return Ok(new ApiResponseDto<bool>(ok));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpPost("{id:int}/transition")]
        [LogUserActivity(AuditOperationConstants.Update, "Transitioned bid {id}")]
        public async Task<IActionResult> Transition(int id, [FromBody] BidStatusTransitionDto dto)
        {
            try
            {
                var bid = await _bidManager.TransitionStatusAsync(id, dto.TargetStatusId, dto.Note, UserId, Language);
                return Ok(new ApiResponseDto<BidDto>(bid));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("{id:int}/allowed-next-statuses")]
        public async Task<IActionResult> AllowedNextStatuses(int id)
        {
            try
            {
                var allowed = (await _bidManager.GetAllowedNextStatusesAsync(id))
                    .Select(s => (int)s).ToList();
                return Ok(new ApiResponseDto<List<int>>(allowed));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
    }
}
