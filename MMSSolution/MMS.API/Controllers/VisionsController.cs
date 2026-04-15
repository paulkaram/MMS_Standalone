using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.BLL.Managers;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DTO;
using MMS.DTO.Bids;

namespace MMS.API.Controllers
{
    /// <summary>
    /// Visions/Reviews (المرئيات) — stakeholder opinions on bid items.
    /// Created automatically when a bid enters VisionPreparation; the bid
    /// cannot move to VisionsCompleted until every vision is Submitted.
    /// </summary>
    [Route("api/visions")]
    [ApiController]
    public class VisionsController : IntalioBaseController
    {
        private readonly VisionManager _visionManager;
        private readonly WorkflowEngine _workflowEngine;
        private readonly IMMSUnitOfWork _uow;

        public VisionsController(VisionManager visionManager, WorkflowEngine workflowEngine, IMMSUnitOfWork uow)
        {
            _visionManager = visionManager;
            _workflowEngine = workflowEngine;
            _uow = uow;
        }

        [HttpGet("bid/{bidId:int}")]
        public async Task<IActionResult> ListByBid(int bidId)
        {
            try
            {
                var visions = await _visionManager.ListByBidAsync(bidId, Language);
                return Ok(new ApiResponseDto<List<BidItemVisionDto>>(visions));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("bid/{bidId:int}/summary")]
        public async Task<IActionResult> GetSummary(int bidId)
        {
            try
            {
                var summary = await _visionManager.GetSummaryAsync(bidId, Language);
                return Ok(new ApiResponseDto<BidVisionsSummaryDto>(summary));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> ListMine([FromQuery] int? bidId = null)
        {
            try
            {
                var visions = await _visionManager.ListForStakeholderAsync(UserId, bidId, Language);
                return Ok(new ApiResponseDto<List<BidItemVisionDto>>(visions));
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
                var vision = await _visionManager.GetAsync(id, Language);
                if (vision == null) return NotFound();
                return Ok(new ApiResponseDto<BidItemVisionDto>(vision));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> SaveDraft(int id, [FromBody] BidItemVisionPostDto dto)
        {
            try
            {
                var vision = await _visionManager.SaveDraftAsync(id, dto, UserId, Language);
                return Ok(new ApiResponseDto<BidItemVisionDto>(vision));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }

        [HttpPost("{id:int}/submit")]
        public async Task<IActionResult> Submit(int id, [FromBody] BidItemVisionPostDto dto)
        {
            try
            {
                var vision = await _visionManager.SubmitAsync(id, dto, UserId, Language);

                // If this was the last stakeholder's last vision, fire the Auto
                // transition to move the workflow to VisionsReview (§5.6 step 4).
                if (await _visionManager.AreAllSubmittedAsync(vision.BidId))
                {
                    var instance = await _uow.WorkflowInstances.GetByBidAsync(vision.BidId);
                    if (instance != null)
                    {
                        await _workflowEngine.TryAutoAdvanceAsync(instance.Id, UserId);
                    }
                }
                return Ok(new ApiResponseDto<BidItemVisionDto>(vision));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex);
            }
        }
    }
}
