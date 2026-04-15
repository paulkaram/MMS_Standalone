using MapsterMapper;
using MMS.BLL.Constants;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO.Bids;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    /// <summary>
    /// Handles the Visions/Reviews (المرئيات) workflow for bids (§5.8).
    /// Each bid stakeholder must submit a vision for every bid item before the
    /// bid can transition from VisionPreparation to VisionsCompleted.
    /// </summary>
    public class VisionManager
    {
        private readonly IMapper _mapper;
        private readonly IMMSUnitOfWork _mmsUnitOfWork;

        public VisionManager(IMapper mapper, IMMSUnitOfWork mmsUnitOfWork)
        {
            _mapper = mapper;
            _mmsUnitOfWork = mmsUnitOfWork;
        }

        /// <summary>
        /// Fans out vision rows when a bid enters VisionPreparation:
        /// one draft row per (stakeholder x bid item). Idempotent.
        /// </summary>
        public async Task InitiateVisionsAsync(int bidId, string initiatorUserId)
        {
            var bid = await _mmsUnitOfWork.Bids.GetIncludeAllAsync(bidId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            var existing = (await _mmsUnitOfWork.BidItemVisions.ListByBidAsync(bidId)).ToList();
            var existingKeys = existing
                .Select(v => (v.BidItemId, v.StakeholderUserId, v.ExternalMemberId))
                .ToHashSet();

            foreach (var item in bid.Items)
            {
                foreach (var sh in bid.Stakeholders)
                {
                    var key = (item.Id, sh.UserId, sh.ExternalMemberId);
                    if (existingKeys.Contains(key)) continue;

                    await _mmsUnitOfWork.BidItemVisions.AddAsync(new BidItemVision
                    {
                        BidId = bidId,
                        BidItemId = item.Id,
                        StakeholderUserId = sh.UserId,
                        ExternalMemberId = sh.ExternalMemberId,
                        StatusId = (int)VisionStatusDbEnum.Draft,
                        CreatedBy = initiatorUserId,
                        CreatedDate = DateTime.Now
                    });
                }
            }

            await _mmsUnitOfWork.SaveChangesAsync();
        }

        public async Task<List<BidItemVisionDto>> ListByBidAsync(int bidId, LanguageDbEnum language)
        {
            var visions = (await _mmsUnitOfWork.BidItemVisions.ListByBidAsync(bidId)).ToList();
            return visions.Select(v => _mapper.Map<BidItemVisionDto>((v, language))).ToList();
        }

        public async Task<List<BidItemVisionDto>> ListForStakeholderAsync(string userId, int? bidId, LanguageDbEnum language)
        {
            var visions = (await _mmsUnitOfWork.BidItemVisions.ListForStakeholderAsync(userId, bidId)).ToList();
            // Reload with full includes per row (simpler than expanding the repo method)
            var result = new List<BidItemVisionDto>();
            foreach (var v in visions)
            {
                var full = await _mmsUnitOfWork.BidItemVisions.GetIncludeAllAsync(v.Id);
                if (full != null) result.Add(_mapper.Map<BidItemVisionDto>((full, language)));
            }
            return result;
        }

        public async Task<BidItemVisionDto?> GetAsync(int id, LanguageDbEnum language)
        {
            var v = await _mmsUnitOfWork.BidItemVisions.GetIncludeAllAsync(id);
            return v == null ? null : _mapper.Map<BidItemVisionDto>((v, language));
        }

        public async Task<BidItemVisionDto> SaveDraftAsync(int visionId, BidItemVisionPostDto dto, string userId, LanguageDbEnum language)
        {
            var vision = await LoadAndAuthorizeAsync(visionId, userId);

            if (vision.StatusId == (int)VisionStatusDbEnum.Submitted)
                throw new InvalidOperationException(MessageConstants.ErrorOccured);

            vision.Comment = dto.Comment;
            vision.UpdatedBy = userId;
            vision.UpdatedDate = DateTime.Now;
            await _mmsUnitOfWork.SaveChangesAsync();

            var full = await _mmsUnitOfWork.BidItemVisions.GetIncludeAllAsync(visionId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
            return _mapper.Map<BidItemVisionDto>((full, language));
        }

        public async Task<BidItemVisionDto> SubmitAsync(int visionId, BidItemVisionPostDto dto, string userId, LanguageDbEnum language)
        {
            var vision = await LoadAndAuthorizeAsync(visionId, userId);

            if (vision.StatusId == (int)VisionStatusDbEnum.Submitted)
                throw new InvalidOperationException(MessageConstants.ErrorOccured);

            vision.Comment = dto.Comment;
            vision.StatusId = (int)VisionStatusDbEnum.Submitted;
            vision.SubmittedDate = DateTime.Now;
            vision.UpdatedBy = userId;
            vision.UpdatedDate = DateTime.Now;
            await _mmsUnitOfWork.SaveChangesAsync();

            var full = await _mmsUnitOfWork.BidItemVisions.GetIncludeAllAsync(visionId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
            return _mapper.Map<BidItemVisionDto>((full, language));
        }

        /// <summary>
        /// True only when every vision row for this bid is in Submitted state
        /// and at least one row exists. Gates the transition to VisionsCompleted.
        /// </summary>
        public async Task<bool> AreAllSubmittedAsync(int bidId)
        {
            var total = await _mmsUnitOfWork.BidItemVisions.CountByBidAsync(bidId);
            if (total == 0) return false;
            var submitted = await _mmsUnitOfWork.BidItemVisions.CountByBidAndStatusAsync(bidId, (int)VisionStatusDbEnum.Submitted);
            return submitted == total;
        }

        public async Task<BidVisionsSummaryDto> GetSummaryAsync(int bidId, LanguageDbEnum language)
        {
            var visions = (await _mmsUnitOfWork.BidItemVisions.ListByBidAsync(bidId)).ToList();
            var total = visions.Count;
            var submitted = visions.Count(v => v.StatusId == (int)VisionStatusDbEnum.Submitted);

            var grouped = visions
                .GroupBy(v => (v.StakeholderUserId, v.ExternalMemberId))
                .Select(g =>
                {
                    var first = g.First();
                    var name = first.StakeholderUser != null
                        ? (language == LanguageDbEnum.Arabic ? first.StakeholderUser.FullnameAr : first.StakeholderUser.FullnameEn)
                        : first.ExternalMember != null
                            ? (language == LanguageDbEnum.Arabic ? first.ExternalMember.FullnameAr : first.ExternalMember.FullnameEn)
                            : string.Empty;

                    var groupTotal = g.Count();
                    var groupSubmitted = g.Count(v => v.StatusId == (int)VisionStatusDbEnum.Submitted);

                    return new BidVisionStakeholderProgressDto
                    {
                        UserId = first.StakeholderUserId,
                        ExternalMemberId = first.ExternalMemberId,
                        Name = name ?? string.Empty,
                        IsExternal = first.ExternalMemberId != null,
                        Total = groupTotal,
                        Submitted = groupSubmitted,
                        Completed = groupTotal > 0 && groupSubmitted == groupTotal
                    };
                })
                .OrderBy(p => p.Name)
                .ToList();

            var stakeholdersCount = grouped.Count;
            var itemsCount = visions.Select(v => v.BidItemId).Distinct().Count();

            return new BidVisionsSummaryDto
            {
                BidId = bidId,
                TotalVisions = total,
                SubmittedVisions = submitted,
                StakeholdersCount = stakeholdersCount,
                ItemsCount = itemsCount,
                AllSubmitted = total > 0 && submitted == total,
                ByStakeholder = grouped
            };
        }

        /// <summary>
        /// Loads a vision and ensures the acting user owns it.
        /// </summary>
        private async Task<BidItemVision> LoadAndAuthorizeAsync(int visionId, string userId)
        {
            var vision = await _mmsUnitOfWork.BidItemVisions.GetAsync(v => v.Id == visionId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            if (vision.StakeholderUserId != userId)
                throw new UnauthorizedAccessException();

            return vision;
        }
    }
}
