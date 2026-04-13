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

        [HttpGet("{id}")]
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
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPut("{id}")]
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

        [HttpDelete("{id}")]
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

        [HttpPost("{id}/transition")]
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

        [HttpGet("{id}/allowed-next-statuses")]
        public IActionResult AllowedNextStatuses(int id)
        {
            try
            {
                // Load the bid first to get current status
                // For simplicity, let the caller pass current status via separate call
                // Here we'll compute from the bid
                var bid = _bidManager.GetAsync(id, Language).Result;
                if (bid == null)
                    return NotFound(new ApiResponseDto<object>(Success: false, Message: MessageConstants.ErrorOccured));

                var allowed = _bidManager.GetAllowedNextStatuses(bid.StatusId)
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
