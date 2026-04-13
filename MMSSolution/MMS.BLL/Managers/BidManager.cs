using MapsterMapper;
using MMS.BLL.Constants;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.Bids;
using MMS.DTO.CommitteeItems;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    public class BidManager
    {
        private readonly IMapper _mapper;
        private readonly IMMSUnitOfWork _mmsUnitOfWork;

        // Valid status transitions — ensures users follow the 13-step lifecycle
        private static readonly Dictionary<BidStatusDbEnum, BidStatusDbEnum[]> AllowedTransitions = new()
        {
            { BidStatusDbEnum.Draft, new[] { BidStatusDbEnum.PendingManagerApproval } },
            { BidStatusDbEnum.PendingManagerApproval, new[] { BidStatusDbEnum.VisionPreparation, BidStatusDbEnum.Returned } },
            { BidStatusDbEnum.VisionPreparation, new[] { BidStatusDbEnum.VisionsCompleted, BidStatusDbEnum.Returned } },
            { BidStatusDbEnum.VisionsCompleted, new[] { BidStatusDbEnum.PreparatoryMeeting, BidStatusDbEnum.Returned } },
            { BidStatusDbEnum.PreparatoryMeeting, new[] { BidStatusDbEnum.MinisterialMeeting } },
            { BidStatusDbEnum.MinisterialMeeting, new[] { BidStatusDbEnum.ExternalMeetingDone } },
            { BidStatusDbEnum.ExternalMeetingDone, new[] { BidStatusDbEnum.AwaitingOpinion } },
            { BidStatusDbEnum.AwaitingOpinion, new[] { BidStatusDbEnum.FinalMinutes } },
            { BidStatusDbEnum.FinalMinutes, new[] { BidStatusDbEnum.AssignmentsCreated } },
            { BidStatusDbEnum.AssignmentsCreated, new[] { BidStatusDbEnum.Completed } },
            { BidStatusDbEnum.Returned, new[] { BidStatusDbEnum.Draft, BidStatusDbEnum.PendingManagerApproval } },
            { BidStatusDbEnum.Completed, Array.Empty<BidStatusDbEnum>() }
        };

        public BidManager(IMapper mapper, IMMSUnitOfWork mmsUnitOfWork)
        {
            _mapper = mapper;
            _mmsUnitOfWork = mmsUnitOfWork;
        }

        public async Task<List<BidStatusDto>> ListStatusesAsync()
        {
            var statuses = (await _mmsUnitOfWork.BidStatuses.ListAsync())
                .OrderBy(s => s.StepOrder)
                .ToList();
            return statuses.Select(s => _mapper.Map<BidStatusDto>(s)).ToList();
        }

        public async Task<List<BidDto>> ListByCommitteeAsync(int committeeId, LanguageDbEnum language)
        {
            var bids = (await _mmsUnitOfWork.Bids.ListByCommitteeAsync(committeeId)).ToList();
            var now = DateTime.Now;
            return bids.Select(b => _mapper.Map<BidDto>((b, language, now))).ToList();
        }

        public async Task<List<BidDto>> ListForUserAsync(string userId, LanguageDbEnum language)
        {
            // Committees the user is a member of (via UserCommittee)
            var memberCommitteeIds = (await _mmsUnitOfWork.UserCommittee.ListAsync(uc => uc.UserId == userId))
                .Select(uc => uc.CommitteeId).Distinct().ToList();

            // Bids this user: created, is team leader, is stakeholder, or belongs to a committee they're in
            var allBids = await _mmsUnitOfWork.Bids.ListAsync();

            var stakeholderBidIds = (await _mmsUnitOfWork.BidStakeholders.ListAsync(s => s.UserId == userId))
                .Select(s => s.BidId).Distinct().ToHashSet();

            var relevantBids = allBids.Where(b =>
                b.CreatedBy == userId
                || b.TeamLeaderUserId == userId
                || memberCommitteeIds.Contains(b.CommitteeId)
                || stakeholderBidIds.Contains(b.Id)
            ).ToList();

            // Reload with includes
            var ids = relevantBids.Select(b => b.Id).ToList();
            var enriched = new List<Bid>();
            foreach (var id in ids)
            {
                var b = await _mmsUnitOfWork.Bids.GetIncludeAllAsync(id);
                if (b != null) enriched.Add(b);
            }

            var now = DateTime.Now;
            return enriched
                .OrderByDescending(b => b.CreatedDate)
                .Select(b => _mapper.Map<BidDto>((b, language, now)))
                .ToList();
        }

        public async Task<BidDetailDto?> GetAsync(int id, LanguageDbEnum language)
        {
            var bid = await _mmsUnitOfWork.Bids.GetIncludeAllAsync(id);
            if (bid == null) return null;

            var dto = _mapper.Map<BidDto>((bid, language, DateTime.Now));
            var detail = new BidDetailDto
            {
                Id = dto.Id,
                CommitteeId = dto.CommitteeId,
                CommitteeName = dto.CommitteeName,
                ReferenceNumber = dto.ReferenceNumber,
                ExternalMeetingNumber = dto.ExternalMeetingNumber,
                Subject = dto.Subject,
                Description = dto.Description,
                TeamLeaderUserId = dto.TeamLeaderUserId,
                TeamLeaderName = dto.TeamLeaderName,
                StatusId = dto.StatusId,
                StatusName = dto.StatusName,
                StatusStepOrder = dto.StatusStepOrder,
                StartDate = dto.StartDate,
                DueDate = dto.DueDate,
                MeetingId = dto.MeetingId,
                InitialMinutesPath = dto.InitialMinutesPath,
                FinalMinutesPath = dto.FinalMinutesPath,
                CreatedBy = dto.CreatedBy,
                CreatedByName = dto.CreatedByName,
                CreatedDate = dto.CreatedDate,
                StakeholdersCount = dto.StakeholdersCount,
                ItemsCount = dto.ItemsCount,
                IsOverdue = dto.IsOverdue,
                Stakeholders = bid.Stakeholders
                    .Select(s => _mapper.Map<BidStakeholderDto>((s, language)))
                    .ToList(),
                History = bid.StatusHistory
                    .OrderByDescending(h => h.ChangedDate)
                    .Select(h => _mapper.Map<BidStatusHistoryDto>((h, language)))
                    .ToList(),
                Items = bid.Items
                    .OrderBy(i => i.Order).ThenBy(i => i.Id)
                    .Select(i => _mapper.Map<CommitteeItemDto>((i, language)))
                    .ToList()
            };
            return detail;
        }

        public async Task<BidDto> CreateAsync(BidPostDto dto, string userId, LanguageDbEnum language)
        {
            ValidatePostDto(dto);

            var bid = new Bid
            {
                CommitteeId = dto.CommitteeId,
                ReferenceNumber = await GenerateReferenceNumberAsync(dto.CommitteeId),
                ExternalMeetingNumber = dto.ExternalMeetingNumber,
                Subject = dto.Subject,
                Description = dto.Description,
                TeamLeaderUserId = dto.TeamLeaderUserId,
                StatusId = (int)BidStatusDbEnum.Draft,
                StartDate = dto.StartDate,
                DueDate = dto.DueDate,
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            };

            await _mmsUnitOfWork.Bids.AddAsync(bid);
            await _mmsUnitOfWork.SaveChangesAsync();

            if (dto.Stakeholders.Any())
            {
                foreach (var sh in dto.Stakeholders)
                {
                    if (string.IsNullOrEmpty(sh.UserId) && sh.ExternalMemberId == null) continue;
                    await _mmsUnitOfWork.BidStakeholders.AddAsync(new BidStakeholder
                    {
                        BidId = bid.Id,
                        UserId = sh.UserId,
                        ExternalMemberId = sh.ExternalMemberId,
                        IsTeamLeader = sh.IsTeamLeader,
                        CreatedDate = DateTime.Now
                    });
                }
                await _mmsUnitOfWork.SaveChangesAsync();
            }

            // Record initial status history
            await RecordHistoryAsync(bid.Id, null, (int)BidStatusDbEnum.Draft, userId, null);

            var reloaded = await _mmsUnitOfWork.Bids.GetIncludeAllAsync(bid.Id)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
            return _mapper.Map<BidDto>((reloaded, language, DateTime.Now));
        }

        public async Task<BidDto?> UpdateAsync(int id, BidPostDto dto, LanguageDbEnum language)
        {
            ValidatePostDto(dto);

            var bid = await _mmsUnitOfWork.Bids.GetAsync(b => b.Id == id);
            if (bid == null) return null;

            // Allow edits only in Draft or Returned state
            if (bid.StatusId != (int)BidStatusDbEnum.Draft && bid.StatusId != (int)BidStatusDbEnum.Returned)
                throw new InvalidOperationException(MessageConstants.ErrorOccured);

            bid.ExternalMeetingNumber = dto.ExternalMeetingNumber;
            bid.Subject = dto.Subject;
            bid.Description = dto.Description;
            bid.TeamLeaderUserId = dto.TeamLeaderUserId;
            bid.StartDate = dto.StartDate;
            bid.DueDate = dto.DueDate;

            await _mmsUnitOfWork.SaveChangesAsync();

            // Replace stakeholders
            await _mmsUnitOfWork.BidStakeholders.RemoveAllForBidAsync(id);
            foreach (var sh in dto.Stakeholders)
            {
                if (string.IsNullOrEmpty(sh.UserId) && sh.ExternalMemberId == null) continue;
                await _mmsUnitOfWork.BidStakeholders.AddAsync(new BidStakeholder
                {
                    BidId = id,
                    UserId = sh.UserId,
                    ExternalMemberId = sh.ExternalMemberId,
                    IsTeamLeader = sh.IsTeamLeader,
                    CreatedDate = DateTime.Now
                });
            }
            await _mmsUnitOfWork.SaveChangesAsync();

            var reloaded = await _mmsUnitOfWork.Bids.GetIncludeAllAsync(id);
            return reloaded == null ? null : _mapper.Map<BidDto>((reloaded, language, DateTime.Now));
        }

        public async Task<bool> DeleteAsync(int id, string requestingUserId)
        {
            var bid = await _mmsUnitOfWork.Bids.GetAsync(b => b.Id == id);
            if (bid == null) return false;

            // Only creator can delete, only in Draft
            if (bid.CreatedBy != requestingUserId)
                throw new UnauthorizedAccessException();
            if (bid.StatusId != (int)BidStatusDbEnum.Draft)
                throw new InvalidOperationException(MessageConstants.ErrorOccured);

            _mmsUnitOfWork.Bids.Remove(bid);
            await _mmsUnitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<BidDto> TransitionStatusAsync(int bidId, int targetStatusId, string? note, string userId, LanguageDbEnum language)
        {
            var bid = await _mmsUnitOfWork.Bids.GetAsync(b => b.Id == bidId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            var currentStatus = (BidStatusDbEnum)bid.StatusId;
            var targetStatus = (BidStatusDbEnum)targetStatusId;

            if (!AllowedTransitions.TryGetValue(currentStatus, out var allowed) || !allowed.Contains(targetStatus))
                throw new InvalidOperationException(MessageConstants.ErrorOccured);

            var fromStatusId = bid.StatusId;
            bid.StatusId = targetStatusId;
            await _mmsUnitOfWork.SaveChangesAsync();

            await RecordHistoryAsync(bidId, fromStatusId, targetStatusId, userId, note);

            var reloaded = await _mmsUnitOfWork.Bids.GetIncludeAllAsync(bidId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
            return _mapper.Map<BidDto>((reloaded, language, DateTime.Now));
        }

        public IReadOnlyList<BidStatusDbEnum> GetAllowedNextStatuses(int currentStatusId)
        {
            var current = (BidStatusDbEnum)currentStatusId;
            return AllowedTransitions.TryGetValue(current, out var allowed) ? allowed : Array.Empty<BidStatusDbEnum>();
        }

        private async Task<string> GenerateReferenceNumberAsync(int committeeId)
        {
            int year = DateTime.Now.Year;
            int sequence = await _mmsUnitOfWork.Bids.GetNextSequenceAsync(committeeId, year);
            return $"{FormattingConstants.BidReferenceNumberPrefix}-{year}-{committeeId}-{sequence}";
        }

        private async Task RecordHistoryAsync(int bidId, int? fromStatusId, int toStatusId, string userId, string? note)
        {
            await _mmsUnitOfWork.BidStatusHistory.AddAsync(new BidStatusHistory
            {
                BidId = bidId,
                FromStatusId = fromStatusId,
                ToStatusId = toStatusId,
                ChangedBy = userId,
                ChangedDate = DateTime.Now,
                Note = note
            });
            await _mmsUnitOfWork.SaveChangesAsync();
        }

        private static void ValidatePostDto(BidPostDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Subject))
                throw new ArgumentException(MessageConstants.ErrorOccured);
            if (dto.CommitteeId <= 0)
                throw new ArgumentException(MessageConstants.ErrorOccured);
            if (dto.DueDate < dto.StartDate)
                throw new ArgumentException(MessageConstants.ErrorOccured);
        }
    }
}
