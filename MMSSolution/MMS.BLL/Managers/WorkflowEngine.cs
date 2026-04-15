using Microsoft.EntityFrameworkCore;
using MMS.BLL.Constants;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    /// <summary>
    /// Core RBAC workflow engine. Resolves assignees for each step based on
    /// ActorSourceType (Role / Group / CommitteeRole / Permission / Stakeholder /
    /// TeamLeader / Creator / SpecificUser), fires transitions, and manages
    /// WorkflowTask lifecycle.
    ///
    /// Nothing about "who acts at which step" is hardcoded — everything flows
    /// from the WorkflowTemplate configuration.
    /// </summary>
    public class WorkflowEngine
    {
        private readonly IMMSUnitOfWork _uow;
        private readonly VisionManager _visionManager;
        private readonly OpinionManager _opinionManager;

        public WorkflowEngine(IMMSUnitOfWork uow, VisionManager visionManager, OpinionManager opinionManager)
        {
            _uow = uow;
            _visionManager = visionManager;
            _opinionManager = opinionManager;
        }

        // ════════════════════════════════════════════════════════════════
        //  INSTANCE LIFECYCLE
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Starts a workflow for a newly created bid. Picks the committee's
        /// template if configured, otherwise the global default.
        /// </summary>
        public async Task<WorkflowInstance> StartInstanceForBidAsync(Bid bid, string initiatorUserId)
        {
            var existing = await _uow.WorkflowInstances.GetByBidAsync(bid.Id);
            if (existing != null) return existing;

            var template = await _uow.WorkflowTemplates.GetForCommitteeAsync(bid.CommitteeId)
                        ?? await _uow.WorkflowTemplates.GetGlobalDefaultAsync()
                        ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            // Entry step = lowest StepOrder
            var firstStep = template.Steps.OrderBy(s => s.StepOrder).FirstOrDefault()
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            var instance = new WorkflowInstance
            {
                TemplateId = template.Id,
                BidId = bid.Id,
                CurrentStepId = firstStep.Id,
                StartedDate = DateTime.UtcNow,
                StartedBy = initiatorUserId
            };
            await _uow.WorkflowInstances.AddAsync(instance);
            await _uow.SaveChangesAsync();

            // Record the entry. Create a task only if the first step isn't a "self-edit"
            // draft — when the step's actor is the bid's creator, they're already on the
            // bid detail page authoring the bid and don't need a redundant task on Tasks.
            await RecordHistoryAsync(instance.Id, fromStepId: null, toStepId: firstStep.Id, transitionId: null, userId: initiatorUserId, note: null);
            var firstActorIsCreator = firstStep.ActorSourceType == (int)ActorSourceTypeDbEnum.Creator;
            if (!firstActorIsCreator)
            {
                await CreateTasksForStepAsync(instance, firstStep, bid);
            }
            await _uow.SaveChangesAsync();

            return instance;
        }

        /// <summary>
        /// Fires a transition. Validates that:
        ///  (1) the transition belongs to this instance's template
        ///  (2) the transition starts from the current step
        ///  (3) the acting user is one of the assignees for the current step
        /// Then advances the instance, cancels leftover tasks, creates new tasks,
        /// and completes the instance if the new step is terminal.
        /// </summary>
        public async Task<WorkflowInstance> FireTransitionAsync(int instanceId, int transitionId, string userId, string? note, int? linkedMeetingId = null)
        {
            var instance = await _uow.WorkflowInstances.GetWithTemplateAsync(instanceId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            if (instance.CompletedDate != null)
                throw new InvalidOperationException(MessageConstants.ErrorOccured);

            var transition = instance.Template.Transitions.FirstOrDefault(t => t.Id == transitionId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            if (transition.FromStepId != instance.CurrentStepId)
                throw new InvalidOperationException(MessageConstants.ErrorOccured);

            var bid = await _uow.Bids.GetIncludeAllAsync(instance.BidId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            // Authorization: is this user a valid actor on the current step?
            var currentStep = instance.Template.Steps.First(s => s.Id == instance.CurrentStepId);
            if (transition.ActionType != (int)WorkflowActionTypeDbEnum.Auto)
            {
                var actorUserIds = await ResolveActorUserIdsAsync(currentStep, bid);
                if (!actorUserIds.Contains(userId))
                    throw new UnauthorizedAccessException();
            }

            var targetStep = instance.Template.Steps.FirstOrDefault(s => s.Id == transition.ToStepId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            // ── Domain gate: can't complete the visions stage unless every stakeholder
            //    has submitted a vision on every item. The engine stays generic by
            //    keying off LegacyBidStatusId; when we split BidItem fully in §5.11
            //    this hook becomes a named callback instead of a status check.
            if (targetStep.LegacyBidStatusId == (int)BidStatusDbEnum.VisionsCompleted
                && !await _visionManager.AreAllSubmittedAsync(instance.BidId))
            {
                throw new InvalidOperationException(MessageConstants.ErrorOccured);
            }

            // ── Domain gate: can't move to FinalMinutes (§5.6 step 9) unless every
            //    stakeholder has submitted their Suitable/Unsuitable opinion.
            if (targetStep.LegacyBidStatusId == (int)BidStatusDbEnum.FinalMinutes
                && !await _opinionManager.AreAllSubmittedAsync(instance.BidId))
            {
                throw new InvalidOperationException(MessageConstants.ErrorOccured);
            }

            // Cancel any open tasks from the current step — their owners no longer need to act.
            await _uow.WorkflowTasks.CancelPendingForInstanceAsync(instance.Id);

            // Advance
            instance.CurrentStepId = targetStep.Id;
            if (targetStep.IsTerminal) instance.CompletedDate = DateTime.UtcNow;

            // Keep Bid.StatusId in sync via LegacyBidStatusId so legacy code paths keep working
            if (targetStep.LegacyBidStatusId.HasValue)
                bid.StatusId = targetStep.LegacyBidStatusId.Value;

            // Optional meeting linkage (§5.6 steps 5/6 — preparatory / ministerial prep)
            if (linkedMeetingId.HasValue && linkedMeetingId.Value > 0)
                bid.MeetingId = linkedMeetingId.Value;

            await _uow.SaveChangesAsync();

            await RecordHistoryAsync(instance.Id, fromStepId: transition.FromStepId, toStepId: targetStep.Id, transitionId: transition.Id, userId: userId, note: note);

            if (!targetStep.IsTerminal)
                await CreateTasksForStepAsync(instance, targetStep, bid);

            // ── Domain side-effect: entering VisionPreparation fans out one draft
            //    BidItemVision row per (stakeholder × bid item). Idempotent.
            if (targetStep.LegacyBidStatusId == (int)BidStatusDbEnum.VisionPreparation)
            {
                await _visionManager.InitiateVisionsAsync(instance.BidId, userId);
            }

            // ── Domain side-effect: entering AwaitingOpinion (§5.6 step 10) fans
            //    out one Draft opinion row per stakeholder for Suitable/Unsuitable voting.
            if (targetStep.LegacyBidStatusId == (int)BidStatusDbEnum.AwaitingOpinion)
            {
                await _opinionManager.InitiateOpinionsAsync(instance.BidId, userId);
            }

            await _uow.SaveChangesAsync();
            return instance;
        }

        /// <summary>
        /// Auto-fires any transition of ActionType=Auto whose source is the
        /// current step. Used by domain callbacks (e.g., "all visions submitted").
        /// </summary>
        public async Task<bool> TryAutoAdvanceAsync(int instanceId, string userId)
        {
            var instance = await _uow.WorkflowInstances.GetWithTemplateAsync(instanceId);
            if (instance == null || instance.CompletedDate != null) return false;

            var autoTransition = instance.Template.Transitions.FirstOrDefault(t =>
                t.FromStepId == instance.CurrentStepId
                && t.ActionType == (int)WorkflowActionTypeDbEnum.Auto);

            if (autoTransition == null) return false;

            await FireTransitionAsync(instanceId, autoTransition.Id, userId, note: null);
            return true;
        }

        // ════════════════════════════════════════════════════════════════
        //  ACTOR RESOLUTION — the RBAC core
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Given a step and a bid, returns all user IDs who should receive a
        /// task (or equivalently: can fire a transition from that step).
        /// </summary>
        public async Task<HashSet<string>> ResolveActorUserIdsAsync(WorkflowStep step, Bid bid)
        {
            var source = (ActorSourceTypeDbEnum)step.ActorSourceType;

            switch (source)
            {
                case ActorSourceTypeDbEnum.Role:
                    {
                        if (!int.TryParse(step.ActorTargetId, out var roleId)) return new();
                        var users = await _uow.UserRoles.ListAsync(ur => ur.RoleId == roleId);
                        return users.Select(u => u.UserId).ToHashSet();
                    }

                case ActorSourceTypeDbEnum.Group:
                    {
                        if (!int.TryParse(step.ActorTargetId, out var groupId)) return new();
                        var users = await _uow.UserGroups.ListAsync(ug => ug.GroupId == groupId);
                        return users.Select(u => u.UserId).ToHashSet();
                    }

                case ActorSourceTypeDbEnum.CommitteeRole:
                    {
                        if (!int.TryParse(step.ActorTargetId, out var committeeRoleId)) return new();
                        var members = await _uow.UserCommittee.ListAsync(uc =>
                            uc.CommitteeId == bid.CommitteeId && uc.CommitteeRoleId == committeeRoleId);
                        return members.Select(m => m.UserId).ToHashSet();
                    }

                case ActorSourceTypeDbEnum.Permission:
                    {
                        // The permission tables in this codebase use name-based linkage:
                        //   RoleMenuPermission.RoleName  matches Role.RoleNameEn
                        //   GroupMenuPermission.GroupId  is a string of the int Group.Id
                        // Bridge those to UserRole / UserGroup to find the actual users.
                        if (!int.TryParse(step.ActorTargetId, out var permissionId)) return new();
                        var userIds = new HashSet<string>();

                        var rolePerms = await _uow.RoleMenuPermissions.ListAsync(p => p.PermissionId == permissionId);
                        var roleNames = rolePerms.Select(p => p.RoleName).Distinct().ToList();
                        if (roleNames.Count > 0)
                        {
                            var roles = await _uow.Roles.ListAsync(r => roleNames.Contains(r.RoleNameEn));
                            var roleIds = roles.Select(r => r.Id).ToList();
                            var roleUsers = await _uow.UserRoles.ListAsync(ur => roleIds.Contains(ur.RoleId));
                            foreach (var ru in roleUsers) userIds.Add(ru.UserId);
                        }

                        var groupPerms = await _uow.GroupMenuPermissions.ListAsync(p => p.PermissionId == permissionId);
                        var groupIdStrings = groupPerms.Select(p => p.GroupId).Distinct().ToList();
                        if (groupIdStrings.Count > 0)
                        {
                            var groupIds = groupIdStrings
                                .Select(s => int.TryParse(s, out var i) ? i : 0)
                                .Where(i => i > 0)
                                .ToList();
                            var groupUsers = await _uow.UserGroups.ListAsync(ug => groupIds.Contains(ug.GroupId));
                            foreach (var gu in groupUsers) userIds.Add(gu.UserId);
                        }

                        return userIds;
                    }

                case ActorSourceTypeDbEnum.Stakeholder:
                    return bid.Stakeholders
                        .Where(s => !string.IsNullOrEmpty(s.UserId))
                        .Select(s => s.UserId!)
                        .ToHashSet();

                case ActorSourceTypeDbEnum.TeamLeader:
                    return string.IsNullOrEmpty(bid.TeamLeaderUserId)
                        ? new()
                        : new HashSet<string> { bid.TeamLeaderUserId };

                case ActorSourceTypeDbEnum.Creator:
                    return new HashSet<string> { bid.CreatedBy };

                case ActorSourceTypeDbEnum.SpecificUser:
                    return string.IsNullOrEmpty(step.ActorTargetId)
                        ? new()
                        : new HashSet<string> { step.ActorTargetId };

                default:
                    return new();
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  TASK CREATION — one WorkflowTask per resolved assignee
        // ════════════════════════════════════════════════════════════════

        private async Task CreateTasksForStepAsync(WorkflowInstance instance, WorkflowStep step, Bid bid)
        {
            if (step.IsTerminal) return;

            var assignees = await ResolveActorUserIdsAsync(step, bid);
            if (assignees.Count == 0) return;

            DateTime? due = step.SlaDays.HasValue ? DateTime.UtcNow.AddDays(step.SlaDays.Value) : (DateTime?)null;

            foreach (var userId in assignees)
            {
                await _uow.WorkflowTasks.AddAsync(new WorkflowTask
                {
                    InstanceId = instance.Id,
                    StepId = step.Id,
                    BidId = bid.Id,
                    AssignedToUserId = userId,
                    StatusId = (int)WorkflowTaskStatusDbEnum.Pending,
                    DueDate = due,
                    CreatedDate = DateTime.UtcNow
                });
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  TASK ACTIONS
        // ════════════════════════════════════════════════════════════════

        public async Task<WorkflowTask> ClaimTaskAsync(int taskId, string userId)
        {
            var task = await _uow.WorkflowTasks.GetAsync(t => t.Id == taskId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            if (task.AssignedToUserId != userId)
                throw new UnauthorizedAccessException();

            if (task.StatusId == (int)WorkflowTaskStatusDbEnum.Pending)
            {
                task.StatusId = (int)WorkflowTaskStatusDbEnum.InProgress;
                task.ClaimedDate = DateTime.UtcNow;
                await _uow.SaveChangesAsync();
            }
            return task;
        }

        /// <summary>
        /// Mark a task as completed without firing a transition — used when the
        /// step was completed through a side-channel (e.g., visions fan-out
        /// where the transition is fired separately once all are submitted).
        /// </summary>
        public async Task CompleteTaskAsync(int taskId, string userId, string? note)
        {
            var task = await _uow.WorkflowTasks.GetAsync(t => t.Id == taskId)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);

            if (task.AssignedToUserId != userId)
                throw new UnauthorizedAccessException();

            task.StatusId = (int)WorkflowTaskStatusDbEnum.Completed;
            task.CompletedDate = DateTime.UtcNow;
            task.CompletedBy = userId;
            task.Note = note;
            await _uow.SaveChangesAsync();
        }

        // ════════════════════════════════════════════════════════════════
        //  HISTORY
        // ════════════════════════════════════════════════════════════════

        private async Task RecordHistoryAsync(int instanceId, int? fromStepId, int toStepId, int? transitionId, string userId, string? note)
        {
            await _uow.WorkflowHistory.AddAsync(new WorkflowHistory
            {
                InstanceId = instanceId,
                FromStepId = fromStepId,
                ToStepId = toStepId,
                TransitionId = transitionId,
                ChangedBy = userId,
                ChangedDate = DateTime.UtcNow,
                Note = note
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  INITIATION GATE
        // ════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns true if the user is allowed to start a bid workflow under
        /// the given template (respects Initiator config if set, otherwise
        /// defers to existing Bids.Write permission check at the controller).
        /// </summary>
        public async Task<bool> CanInitiateAsync(WorkflowTemplate template, string userId, int committeeId)
        {
            if (template.InitiatorActorSourceType == null) return true; // no extra gate beyond the controller's RequiredPermission

            // Build a fake "step" so we can reuse ResolveActorUserIdsAsync. We only
            // need the source type + target id — no bid fields are consulted for these
            // actor types (Stakeholder/TeamLeader/Creator wouldn't make sense here).
            var source = (ActorSourceTypeDbEnum)template.InitiatorActorSourceType.Value;
            var targetId = template.InitiatorActorTargetId;

            switch (source)
            {
                case ActorSourceTypeDbEnum.Role:
                    if (!int.TryParse(targetId, out var roleId)) return false;
                    return (await _uow.UserRoles.ListAsync(ur => ur.UserId == userId && ur.RoleId == roleId)).Any();
                case ActorSourceTypeDbEnum.Group:
                    if (!int.TryParse(targetId, out var groupId)) return false;
                    return (await _uow.UserGroups.ListAsync(ug => ug.UserId == userId && ug.GroupId == groupId)).Any();
                case ActorSourceTypeDbEnum.CommitteeRole:
                    if (!int.TryParse(targetId, out var crId)) return false;
                    return (await _uow.UserCommittee.ListAsync(uc => uc.UserId == userId && uc.CommitteeId == committeeId && uc.CommitteeRoleId == crId)).Any();
                case ActorSourceTypeDbEnum.SpecificUser:
                    return targetId == userId;
                default:
                    return true;
            }
        }
    }
}
