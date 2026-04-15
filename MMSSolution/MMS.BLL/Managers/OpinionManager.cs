using MMS.BLL.Constants;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO.Bids;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    /// <summary>
    /// Handles BRD §5.6 step 10 — stakeholders rate external meeting minutes
    /// as Suitable / Unsuitable (مناسبة / غير مناسبة).
    /// Mirrors VisionManager: fan-out at step entry, per-stakeholder submission,
    /// all-submitted gate before the workflow can move to FinalMinutes.
    /// </summary>
    public class OpinionManager
    {
        private readonly IMMSUnitOfWork _uow;

        public OpinionManager(IMMSUnitOfWork uow)
        {
            _uow = uow;
        }

        /// <summary>Create one Draft opinion row per stakeholder for this bid. Idempotent.</summary>
        public async Task InitiateOpinionsAsync(int bidId, string initiatorUserId)
        {
            var bid = await _uow.Bids.GetIncludeAllAsync(bidId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            var existing = (await _uow.BidMinutesOpinions.ListByBidAsync(bidId)).ToList();
            var existingKeys = existing
                .Select(o => (o.StakeholderUserId, o.ExternalMemberId))
                .ToHashSet();

            foreach (var sh in bid.Stakeholders)
            {
                var key = (sh.UserId, sh.ExternalMemberId);
                if (existingKeys.Contains(key)) continue;

                await _uow.BidMinutesOpinions.AddAsync(new BidMinutesOpinion
                {
                    BidId = bidId,
                    StakeholderUserId = sh.UserId,
                    ExternalMemberId = sh.ExternalMemberId,
                    StatusId = (int)VisionStatusDbEnum.Draft,
                    CreatedBy = initiatorUserId,
                    CreatedDate = DateTime.Now
                });
            }
            await _uow.SaveChangesAsync();
        }

        public async Task<List<BidMinutesOpinionDto>> ListByBidAsync(int bidId, LanguageDbEnum language)
        {
            var rows = (await _uow.BidMinutesOpinions.ListByBidAsync(bidId)).ToList();
            return rows.Select(o => MapDto(o, language)).ToList();
        }

        public async Task<BidMinutesOpinionDto> SubmitAsync(int opinionId, BidMinutesOpinionPostDto dto, string userId, LanguageDbEnum language)
        {
            var opinion = await _uow.BidMinutesOpinions.GetAsync(o => o.Id == opinionId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            if (opinion.StakeholderUserId != userId)
                throw new UnauthorizedAccessException();
            if (opinion.StatusId == (int)VisionStatusDbEnum.Submitted)
                throw new InvalidOperationException(MessageConstants.ErrorOccured);
            if (dto.Opinion != (int)MinutesOpinionDbEnum.Suitable && dto.Opinion != (int)MinutesOpinionDbEnum.Unsuitable)
                throw new ArgumentException(MessageConstants.ErrorOccured);

            opinion.Opinion = dto.Opinion;
            opinion.Comment = dto.Comment;
            opinion.StatusId = (int)VisionStatusDbEnum.Submitted;
            opinion.SubmittedDate = DateTime.Now;
            opinion.UpdatedBy = userId;
            opinion.UpdatedDate = DateTime.Now;
            await _uow.SaveChangesAsync();

            var reloaded = (await _uow.BidMinutesOpinions.ListByBidAsync(opinion.BidId)).First(o => o.Id == opinionId);
            return MapDto(reloaded, language);
        }

        /// <summary>True once every opinion row is in Submitted state. Gates the transition to FinalMinutes.</summary>
        public async Task<bool> AreAllSubmittedAsync(int bidId)
        {
            var total = await _uow.BidMinutesOpinions.CountByBidAsync(bidId);
            if (total == 0) return false;
            var submitted = await _uow.BidMinutesOpinions.CountByBidAndStatusAsync(bidId, (int)VisionStatusDbEnum.Submitted);
            return submitted == total;
        }

        public async Task<BidMinutesOpinionsSummaryDto> GetSummaryAsync(int bidId)
        {
            var rows = (await _uow.BidMinutesOpinions.ListByBidAsync(bidId)).ToList();
            var total = rows.Count;
            var submitted = rows.Count(o => o.StatusId == (int)VisionStatusDbEnum.Submitted);
            var suitable = rows.Count(o => o.Opinion == (int)MinutesOpinionDbEnum.Suitable);
            var unsuitable = rows.Count(o => o.Opinion == (int)MinutesOpinionDbEnum.Unsuitable);

            return new BidMinutesOpinionsSummaryDto
            {
                BidId = bidId,
                TotalOpinions = total,
                SubmittedOpinions = submitted,
                SuitableCount = suitable,
                UnsuitableCount = unsuitable,
                AllSubmitted = total > 0 && submitted == total
            };
        }

        private static BidMinutesOpinionDto MapDto(BidMinutesOpinion o, LanguageDbEnum language)
        {
            var name = o.StakeholderUser != null
                ? (language == LanguageDbEnum.Arabic ? o.StakeholderUser.FullnameAr : o.StakeholderUser.FullnameEn)
                : o.ExternalMember != null
                    ? (language == LanguageDbEnum.Arabic ? o.ExternalMember.FullnameAr : o.ExternalMember.FullnameEn)
                    : string.Empty;

            string? opinionName = o.Opinion switch
            {
                (int)MinutesOpinionDbEnum.Suitable   => language == LanguageDbEnum.Arabic ? "مناسبة"     : "Suitable",
                (int)MinutesOpinionDbEnum.Unsuitable => language == LanguageDbEnum.Arabic ? "غير مناسبة" : "Unsuitable",
                _ => null
            };

            return new BidMinutesOpinionDto
            {
                Id = o.Id,
                BidId = o.BidId,
                StakeholderUserId = o.StakeholderUserId,
                ExternalMemberId = o.ExternalMemberId,
                StakeholderName = name ?? string.Empty,
                IsExternal = o.ExternalMemberId != null,
                Opinion = o.Opinion,
                OpinionName = opinionName,
                Comment = o.Comment,
                StatusId = o.StatusId,
                SubmittedDate = o.SubmittedDate
            };
        }
    }
}
