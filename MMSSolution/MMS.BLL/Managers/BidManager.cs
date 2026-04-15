using MapsterMapper;
using Microsoft.AspNetCore.Http;
using MMS.BLL.Constants;
using MMS.BLL.Storage;
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
        private readonly VisionManager _visionManager;
        private readonly WorkflowEngine _workflowEngine;
        private readonly StorageManager _storageManager;

        public BidManager(IMapper mapper, IMMSUnitOfWork mmsUnitOfWork, VisionManager visionManager, WorkflowEngine workflowEngine, StorageManager storageManager)
        {
            _mapper = mapper;
            _mmsUnitOfWork = mmsUnitOfWork;
            _visionManager = visionManager;
            _workflowEngine = workflowEngine;
            _storageManager = storageManager;
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

            // Initiator gate (§5.6): if the bid's workflow template restricts who
            // can start a bid via InitiatorActorSourceType, enforce it before
            // creating any rows. Falls through silently if unconfigured.
            var template = await _mmsUnitOfWork.WorkflowTemplates.GetForCommitteeAsync(dto.CommitteeId)
                        ?? await _mmsUnitOfWork.WorkflowTemplates.GetGlobalDefaultAsync();
            if (template != null && template.InitiatorActorSourceType.HasValue)
            {
                var canInitiate = await _workflowEngine.CanInitiateAsync(template, userId, dto.CommitteeId);
                if (!canInitiate) throw new UnauthorizedAccessException();
            }

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

            // Record initial status history (kept for legacy UI; the canonical trail lives in WorkflowHistory)
            await RecordHistoryAsync(bid.Id, null, (int)BidStatusDbEnum.Draft, userId, null);

            // Start the RBAC workflow instance — the engine will resolve the
            // entry step's actor (from the template config, not from hardcoded logic)
            // and create the initial task(s). The legacy Bid.StatusId remains in
            // sync via WorkflowStep.LegacyBidStatusId.
            var reloadedForWorkflow = await _mmsUnitOfWork.Bids.GetIncludeAllAsync(bid.Id)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
            await _workflowEngine.StartInstanceForBidAsync(reloadedForWorkflow, userId);

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

        /// <summary>
        /// Patch just the bid's description. Used by the inline-edit affordance
        /// on BidDetail so we don't have to re-send the whole BidPostDto (which
        /// would also re-replace stakeholders).
        /// </summary>
        public async Task<bool> UpdateDescriptionAsync(int id, string? description, string userId)
        {
            var bid = await _mmsUnitOfWork.Bids.GetAsync(b => b.Id == id);
            if (bid == null) return false;
            EnsureCanEditItems(bid, userId);   // same shapeable-bid + creator/TL rule

            bid.Description = description;
            await _mmsUnitOfWork.SaveChangesAsync();
            return true;
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

        /// <summary>
        /// Legacy transition endpoint. The canonical path now goes through
        /// <c>WorkflowController.FireTransition</c> which delegates to the
        /// engine. This shim remains for any older client that still calls
        /// /bids/{id}/transition with a target BidStatusId — we translate that
        /// into a WorkflowTransition ID and forward to the engine.
        /// </summary>
        public async Task<BidDto> TransitionStatusAsync(int bidId, int targetStatusId, string? note, string userId, LanguageDbEnum language)
        {
            var instance = await _mmsUnitOfWork.WorkflowInstances.GetByBidAsync(bidId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            // Find the transition from the current step whose ToStep has this legacy BidStatusId
            var transition = instance.Template.Transitions.FirstOrDefault(t =>
                t.FromStepId == instance.CurrentStepId
                && instance.Template.Steps.Any(s => s.Id == t.ToStepId && s.LegacyBidStatusId == targetStatusId))
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            // All domain gates (visions-completed check, vision fan-out on entry)
            // now live inside WorkflowEngine.FireTransitionAsync, so this shim just delegates.
            await _workflowEngine.FireTransitionAsync(instance.Id, transition.Id, userId, note);

            // Keep the legacy BidStatusHistory table in sync for the old UI
            var toStep = instance.Template.Steps.First(s => s.Id == transition.ToStepId);
            await RecordHistoryAsync(bidId, instance.Template.Steps.First(s => s.Id == transition.FromStepId).LegacyBidStatusId, toStep.LegacyBidStatusId ?? targetStatusId, userId, note);

            var reloaded = await _mmsUnitOfWork.Bids.GetIncludeAllAsync(bidId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
            return _mapper.Map<BidDto>((reloaded, language, DateTime.Now));
        }

        /// <summary>
        /// Legacy shim for the BidDetail page. Returns the legacy BidStatusDbEnum
        /// values reachable from the current step. Frontend uses this to render
        /// the old "next status" chooser — new clients should switch to the
        /// workflow instance's AvailableTransitions list.
        /// </summary>
        public async Task<IReadOnlyList<BidStatusDbEnum>> GetAllowedNextStatusesAsync(int bidId)
        {
            var instance = await _mmsUnitOfWork.WorkflowInstances.GetByBidAsync(bidId);
            if (instance == null) return Array.Empty<BidStatusDbEnum>();

            return instance.Template.Transitions
                .Where(t => t.FromStepId == instance.CurrentStepId)
                .Select(t => instance.Template.Steps.First(s => s.Id == t.ToStepId))
                .Where(s => s.LegacyBidStatusId.HasValue)
                .Select(s => (BidStatusDbEnum)s.LegacyBidStatusId!.Value)
                .Distinct()
                .ToList();
        }

        [Obsolete("Use GetAllowedNextStatusesAsync(bidId). Kept for compilation until all callers migrate.")]
        public IReadOnlyList<BidStatusDbEnum> GetAllowedNextStatuses(int currentStatusId)
        {
            return Array.Empty<BidStatusDbEnum>();
        }

        // ════════════════════════════════════════════════════════════════
        //  STAKEHOLDER CRUD (§5.6 — stakeholders drive the visions fan-out)
        // ════════════════════════════════════════════════════════════════

        public async Task<List<BidStakeholderDto>> ListStakeholdersAsync(int bidId, LanguageDbEnum language)
        {
            var bid = await _mmsUnitOfWork.Bids.GetIncludeAllAsync(bidId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
            return bid.Stakeholders
                .Select(s => _mapper.Map<BidStakeholderDto>((s, language)))
                .ToList();
        }

        public async Task<BidStakeholderDto> AddStakeholderAsync(int bidId, BidStakeholderPostDto dto, string userId, LanguageDbEnum language)
        {
            if (string.IsNullOrEmpty(dto.UserId) && dto.ExternalMemberId == null)
                throw new ArgumentException(MessageConstants.ErrorOccured);

            var bid = await _mmsUnitOfWork.Bids.GetAsync(b => b.Id == bidId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
            EnsureCanEditStakeholders(bid, userId);

            // Dedupe: don't add the same user/external-member twice for this bid
            var existing = await _mmsUnitOfWork.BidStakeholders.ListAsync(s =>
                s.BidId == bidId
                && ((dto.UserId != null && s.UserId == dto.UserId)
                    || (dto.ExternalMemberId != null && s.ExternalMemberId == dto.ExternalMemberId)));
            if (existing.Any())
                throw new InvalidOperationException(MessageConstants.ErrorOccured);

            var sh = new BidStakeholder
            {
                BidId = bidId,
                UserId = string.IsNullOrEmpty(dto.UserId) ? null : dto.UserId,
                ExternalMemberId = dto.ExternalMemberId,
                IsTeamLeader = dto.IsTeamLeader,
                CreatedDate = DateTime.Now
            };
            await _mmsUnitOfWork.BidStakeholders.AddAsync(sh);
            await _mmsUnitOfWork.SaveChangesAsync();

            // Reload with navigations for the DTO
            var reloaded = (await _mmsUnitOfWork.BidStakeholders.ListAsync(s => s.Id == sh.Id)).First();
            return _mapper.Map<BidStakeholderDto>((reloaded, language));
        }

        public async Task<bool> RemoveStakeholderAsync(int stakeholderId, string userId)
        {
            var sh = await _mmsUnitOfWork.BidStakeholders.GetAsync(s => s.Id == stakeholderId);
            if (sh == null) return false;

            var bid = await _mmsUnitOfWork.Bids.GetAsync(b => b.Id == sh.BidId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
            EnsureCanEditStakeholders(bid, userId);

            _mmsUnitOfWork.BidStakeholders.Remove(sh);
            await _mmsUnitOfWork.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Same edit-window rule as bid items — and restricted to the bid's
        /// creator or the team leader (stakeholder list shapes the workflow).
        /// </summary>
        private static void EnsureCanEditStakeholders(Bid bid, string userId)
        {
            var status = (BidStatusDbEnum)bid.StatusId;
            var isCreator = bid.CreatedBy == userId;
            var isTeamLeader = bid.TeamLeaderUserId == userId;

            if (!isCreator && !isTeamLeader)
                throw new UnauthorizedAccessException();

            if (status != BidStatusDbEnum.Draft
                && status != BidStatusDbEnum.Returned
                && status != BidStatusDbEnum.VisionPreparation)
            {
                throw new InvalidOperationException(MessageConstants.ErrorOccured);
            }
        }

        /// <summary>
        /// Lightweight external-member picker scoped to the committee's external-member roster (§5.2).
        /// </summary>
        public async Task<List<ListItemDto>> ListExternalMembersForPickerAsync(int committeeId, LanguageDbEnum language)
        {
            var links = await _mmsUnitOfWork.CommitteeExternalMembers.ListAsync(cm => cm.CommitteeId == committeeId);
            var memberIds = links.Select(l => l.ExternalMemberId).Distinct().ToList();
            if (memberIds.Count == 0) return new();

            var members = await _mmsUnitOfWork.ExternalMembers.ListAsync(m => memberIds.Contains(m.Id));
            return members
                .Select(m => new ListItemDto(
                    m.Id.ToString(),
                    language == LanguageDbEnum.Arabic ? m.FullnameAr : m.FullnameEn))
                .OrderBy(x => x.Name)
                .ToList();
        }

        // ════════════════════════════════════════════════════════════════
        //  BID ATTACHMENTS (§5.6 — a bid is "مرفقات وبنود": attachments + items)
        //  Reuses the shared Attachment table via RecordTypeId = Bid, RecordId = bidId
        // ════════════════════════════════════════════════════════════════

        public async Task<List<AttachmentListItemDto>> ListAttachmentsAsync(int bidId, LanguageDbEnum language)
        {
            var attachments = await _mmsUnitOfWork.Attachments.ListIncludePrivacyAndType(a =>
                a.RecordId == bidId
                && a.RecordTypeId == (int)AttachmentRecordTypeDbEnum.Bid
                && !a.Deleted);
            return attachments.Select(a => _mapper.Map<AttachmentListItemDto>((a, language))).ToList();
        }

        public async Task<List<AttachmentListItemDto>> AddAttachmentsAsync(int bidId, IFormFileCollection files, string userId, short privacyId, LanguageDbEnum language)
        {
            var bid = await _mmsUnitOfWork.Bids.GetAsync(b => b.Id == bidId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            // Same shapeable-bid rule as items/stakeholders: only while the bid
            // is being prepared can attachments be added by creator or team leader.
            EnsureCanEditItems(bid, userId);

            var bidDirectory = StorageFactory.GetBidDirectory(bidId);
            var toAdd = new List<Attachment>();

            for (int i = 0; i < files.Count; i++)
            {
                var fileRelativeUrl = $"{bidDirectory}{Guid.NewGuid()}";
                toAdd.Add(new Attachment
                {
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    FileName = files[i].FileName,
                    FileRelativeUrl = fileRelativeUrl,
                    FileSize = (int)files[i].Length,
                    RecordId = bidId,
                    RecordTypeId = (int)AttachmentRecordTypeDbEnum.Bid,
                    Title = files[i].FileName,
                    Version = 1,
                    PrivacyId = privacyId
                });
            }

            await _mmsUnitOfWork.Attachments.AddRangeAsync(toAdd);
            await _mmsUnitOfWork.SaveChangesAsync();

            // Persist the bytes after the rows are saved so we have IDs (storage
            // uses the relative URL, not the Id, but we mirror the meeting pattern).
            for (int i = 0; i < toAdd.Count; i++)
            {
                using var ms = new MemoryStream();
                await files[i].CopyToAsync(ms);
                await _storageManager.SaveToStorage(ms.ToArray(), toAdd[i].Id, toAdd[i].FileRelativeUrl);
            }

            return await ListAttachmentsAsync(bidId, language);
        }

        public async Task<bool> RemoveAttachmentAsync(int attachmentId, string userId)
        {
            var att = await _mmsUnitOfWork.Attachments.GetAsync(a => a.Id == attachmentId);
            if (att == null || att.Deleted) return false;
            if (att.RecordTypeId != (int)AttachmentRecordTypeDbEnum.Bid)
                throw new InvalidOperationException(MessageConstants.ErrorOccured);

            var bid = await _mmsUnitOfWork.Bids.GetAsync(b => b.Id == att.RecordId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
            EnsureCanEditItems(bid, userId);

            att.Deleted = true;
            await _mmsUnitOfWork.SaveChangesAsync();
            return true;
        }

        // ════════════════════════════════════════════════════════════════
        //  COMMITTEE MEMBER PICKER (for team-leader / stakeholders dropdowns)
        //  Scopes user choices to the bid's committee — prevents a creator
        //  from naming someone outside the committee as team leader.
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Item-type dropdown options (§5.7 line 223: يُقرأ / بناءً على طلب…).
        /// Reads from the `CommitteeItemType` lookup — same list used for agenda items.
        /// </summary>
        public async Task<List<ListItemDto>> ListItemTypesAsync(LanguageDbEnum language)
        {
            var types = await _mmsUnitOfWork.CommitteeItemTypes.ListAsync();
            return types
                .Select(t => new ListItemDto(
                    t.Id.ToString(),
                    language == LanguageDbEnum.Arabic ? t.NameAr : t.NameEn))
                .ToList();
        }

        /// <summary>
        /// Related-item picker: list other committee items in the same committee so the
        /// user can link bid items to earlier agenda items / decisions (§5.7 line 224).
        /// Excludes the current bid's items to avoid circular references.
        /// </summary>
        public async Task<List<ListItemDto>> ListRelatableItemsAsync(int bidId, LanguageDbEnum language)
        {
            var bid = await _mmsUnitOfWork.Bids.GetAsync(b => b.Id == bidId);
            if (bid == null) return new();

            var items = await _mmsUnitOfWork.CommitteeItems.ListAsync(i =>
                i.CommitteeId == bid.CommitteeId && i.BidId != bidId);
            return items
                .Select(i => new ListItemDto(
                    i.Id.ToString(),
                    $"{i.ReferenceNumber} — {(i.Content.Length > 80 ? i.Content.Substring(0, 80) + "…" : i.Content)}"))
                .ToList();
        }

        public async Task<List<ListItemDto>> ListCommitteeMembersForPickerAsync(int committeeId, LanguageDbEnum language)
        {
            var members = await _mmsUnitOfWork.UserCommittee.ListIncludeAllAsync(uc => uc.CommitteeId == committeeId);
            if (members == null || members.Count == 0) return new();

            return members
                .Where(m => m.User != null)
                .Select(m => new ListItemDto(
                    m.User!.Id,
                    language == LanguageDbEnum.Arabic ? m.User.FullnameAr : m.User.FullnameEn))
                .GroupBy(x => x.Id).Select(g => g.First())   // dedupe — a user can hold multiple committee roles
                .OrderBy(x => x.Name)
                .ToList();
        }

        // ════════════════════════════════════════════════════════════════
        //  BID ITEM CRUD (§5.6 — bid clauses/items, type-less per §5.7)
        // ════════════════════════════════════════════════════════════════

        public async Task<CommitteeItemDto> AddItemAsync(int bidId, BidItemPostDto dto, string userId, LanguageDbEnum language)
        {
            ValidateItemDto(dto);
            var bid = await _mmsUnitOfWork.Bids.GetAsync(b => b.Id == bidId)
                ?? throw new InvalidOperationException($"Bid {bidId} not found.");

            EnsureCanEditItems(bid, userId);

            var item = new CommitteeItem
            {
                CommitteeId = bid.CommitteeId,
                BidId = bidId,
                ReferenceNumber = string.IsNullOrWhiteSpace(dto.ReferenceNumber) ? await GenerateBidItemReferenceAsync(bidId) : dto.ReferenceNumber,
                ExternalReferenceNumber = dto.ExternalReferenceNumber,
                Content = dto.Content,
                ItemTypeId = dto.ItemTypeId,          // agenda-style (§5.7 line 223)
                BidItemTypeId = dto.BidItemTypeId,    // procurement classification (§5.11)
                RelatedItemId = dto.RelatedItemId,    // §5.7 line 224 — البنود المرتبطة
                InternalNote = dto.InternalNote,
                Order = dto.Order,
                DueDate = dto.DueDate,
                IsPrivate = false,
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            };
            await _mmsUnitOfWork.CommitteeItems.AddAsync(item);
            await _mmsUnitOfWork.SaveChangesAsync();

            await SyncItemTagsAsync(item.Id, dto.TagIds);

            // Reload with navigations so the Mapster mapping has ItemType /
            // RelatedItem / CreatedByNavigation populated — otherwise nav-dependent
            // fields produce a NullReferenceException on response shaping.
            var reloaded = await _mmsUnitOfWork.CommitteeItems.GetIncludeRelationsAsync(i => i.Id == item.Id) ?? item;
            return _mapper.Map<CommitteeItemDto>((reloaded, language));
        }

        public async Task<CommitteeItemDto> UpdateItemAsync(int itemId, BidItemPostDto dto, string userId, LanguageDbEnum language)
        {
            ValidateItemDto(dto);
            var item = await _mmsUnitOfWork.CommitteeItems.GetAsync(i => i.Id == itemId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
            if (item.BidId == null) throw new InvalidOperationException(MessageConstants.ErrorOccured);

            var bid = await _mmsUnitOfWork.Bids.GetAsync(b => b.Id == item.BidId.Value)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
            EnsureCanEditItems(bid, userId);

            item.ReferenceNumber = dto.ReferenceNumber;
            item.ExternalReferenceNumber = dto.ExternalReferenceNumber;
            item.Content = dto.Content;
            item.InternalNote = dto.InternalNote;
            item.Order = dto.Order;
            item.DueDate = dto.DueDate;
            item.BidItemTypeId = dto.BidItemTypeId;
            item.ItemTypeId = dto.ItemTypeId;
            item.RelatedItemId = dto.RelatedItemId;
            await _mmsUnitOfWork.SaveChangesAsync();

            await SyncItemTagsAsync(item.Id, dto.TagIds);

            var reloaded = await _mmsUnitOfWork.CommitteeItems.GetIncludeRelationsAsync(i => i.Id == item.Id) ?? item;
            return _mapper.Map<CommitteeItemDto>((reloaded, language));
        }

        /// <summary>
        /// Replace the tag set for a committee item. Removes existing TagLink rows
        /// and inserts the new ones keyed by EntityTypeId = CommitteeItem.
        /// </summary>
        private async Task SyncItemTagsAsync(int itemId, List<int> tagIds)
        {
            var entityType = (int)TagEntityTypeDbEnum.CommitteeItem;
            var existing = (await _mmsUnitOfWork.TagLinks.ListAsync(l =>
                l.EntityTypeId == entityType && l.EntityId == itemId)).ToList();
            foreach (var link in existing) _mmsUnitOfWork.TagLinks.Remove(link);

            foreach (var tagId in tagIds?.Distinct() ?? Enumerable.Empty<int>())
            {
                await _mmsUnitOfWork.TagLinks.AddAsync(new TagLink
                {
                    TagId = tagId,
                    EntityTypeId = entityType,
                    EntityId = itemId,
                    CreatedDate = DateTime.Now
                });
            }
            await _mmsUnitOfWork.SaveChangesAsync();
        }

        public async Task<bool> DeleteItemAsync(int itemId, string userId)
        {
            var item = await _mmsUnitOfWork.CommitteeItems.GetAsync(i => i.Id == itemId);
            if (item == null) return false;
            if (item.BidId == null) throw new InvalidOperationException(MessageConstants.ErrorOccured);

            var bid = await _mmsUnitOfWork.Bids.GetAsync(b => b.Id == item.BidId.Value)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
            EnsureCanEditItems(bid, userId);

            _mmsUnitOfWork.CommitteeItems.Remove(item);
            await _mmsUnitOfWork.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Items can only be added/edited/removed while the bid is being shaped:
        /// in Draft (creator working on it), Returned (creator revising), or
        /// VisionPreparation (team leader still adjusting before stakeholders submit).
        /// In any other status the bid content is locked.
        /// </summary>
        private static void EnsureCanEditItems(Bid bid, string userId)
        {
            var status = (BidStatusDbEnum)bid.StatusId;
            var isCreator = bid.CreatedBy == userId;
            var isTeamLeader = bid.TeamLeaderUserId == userId;

            if (!isCreator && !isTeamLeader)
                throw new UnauthorizedAccessException(
                    $"Only the bid creator or team leader can edit content. (creator={bid.CreatedBy}, teamLeader={bid.TeamLeaderUserId}, you={userId})");

            if (status != BidStatusDbEnum.Draft
                && status != BidStatusDbEnum.Returned
                && status != BidStatusDbEnum.VisionPreparation)
            {
                throw new InvalidOperationException(
                    $"Bid is in status '{status}' (id={bid.StatusId}); content can only be edited in Draft, Returned, or VisionPreparation.");
            }
        }

        private static void ValidateItemDto(BidItemPostDto dto)
        {
            // TipTap rich-text editor sends '<p></p>' for an "empty" doc. Treat that as empty too.
            var raw = (dto.Content ?? string.Empty).Trim();
            var stripped = System.Text.RegularExpressions.Regex.Replace(raw, "<[^>]+>", string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(stripped))
                throw new ArgumentException("Item content is required.");
        }

        private async Task<string> GenerateBidItemReferenceAsync(int bidId)
        {
            var existing = await _mmsUnitOfWork.CommitteeItems.ListAsync(i => i.BidId == bidId);
            return $"BI-{bidId}-{existing.Count() + 1}";
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
