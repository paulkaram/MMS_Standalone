using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.BLL.Managers;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DTO;
using MMS.DTO.Bids;

namespace MMS.API.Controllers
{
    /// <summary>
    /// BRD §5.6 step 10: stakeholders vote Suitable / Unsuitable on the
    /// external meeting minutes before the workflow can advance to FinalMinutes.
    /// Fan-out happens automatically when the bid enters AwaitingOpinion.
    /// </summary>
    [Route("api/opinions")]
    [ApiController]
    public class OpinionsController : IntalioBaseController
    {
        private readonly OpinionManager _opinionManager;
        private readonly WorkflowEngine _workflowEngine;
        private readonly IMMSUnitOfWork _uow;

        public OpinionsController(OpinionManager opinionManager, WorkflowEngine workflowEngine, IMMSUnitOfWork uow)
        {
            _opinionManager = opinionManager;
            _workflowEngine = workflowEngine;
            _uow = uow;
        }

        [HttpGet("bid/{bidId:int}")]
        public async Task<IActionResult> ListByBid(int bidId)
        {
            try
            {
                var list = await _opinionManager.ListByBidAsync(bidId, Language);
                return Ok(new ApiResponseDto<List<BidMinutesOpinionDto>>(list));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpGet("bid/{bidId:int}/summary")]
        public async Task<IActionResult> GetSummary(int bidId)
        {
            try
            {
                var summary = await _opinionManager.GetSummaryAsync(bidId);
                return Ok(new ApiResponseDto<BidMinutesOpinionsSummaryDto>(summary));
            }
            catch (Exception ex) { return ErrorResponse(ex); }
        }

        [HttpPost("{id:int}/submit")]
        public async Task<IActionResult> Submit(int id, [FromBody] BidMinutesOpinionPostDto dto)
        {
            try
            {
                var opinion = await _opinionManager.SubmitAsync(id, dto, UserId, Language);

                // When every stakeholder has weighed in, the workflow can advance
                // (BRD §5.6 step 10 → step 11). The engine's Auto-transition handles this.
                if (await _opinionManager.AreAllSubmittedAsync(opinion.BidId))
                {
                    var instance = await _uow.WorkflowInstances.GetByBidAsync(opinion.BidId);
                    if (instance != null)
                        await _workflowEngine.TryAutoAdvanceAsync(instance.Id, UserId);
                }

                return Ok(new ApiResponseDto<BidMinutesOpinionDto>(opinion));
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return ErrorResponse(ex); }
        }
    }
}
