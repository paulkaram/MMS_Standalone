using MapsterMapper;
using MMS.BLL.Constants;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO.Workflow;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    /// <summary>
    /// Designer-facing CRUD + read-through of workflow data.
    /// The runtime lives in <see cref="WorkflowEngine"/>; this class only
    /// shapes templates/steps/transitions for the admin UI and exposes the
    /// runtime data (instance + history + tasks) as DTOs.
    /// </summary>
    public class WorkflowManager
    {
        private readonly IMapper _mapper;
        private readonly IMMSUnitOfWork _uow;
        private readonly WorkflowEngine _engine;

        public WorkflowManager(IMapper mapper, IMMSUnitOfWork uow, WorkflowEngine engine)
        {
            _mapper = mapper;
            _uow = uow;
            _engine = engine;
        }

        // ────────────────────────── Designer: Templates ─────────────────────────

        public async Task<List<WorkflowTemplateListItemDto>> ListTemplatesAsync(LanguageDbEnum language)
        {
            var templates = (await _uow.WorkflowTemplates.ListAllAsync()).ToList();
            var result = new List<WorkflowTemplateListItemDto>();
            foreach (var t in templates)
            {
                var stepsCount = (await _uow.WorkflowSteps.ListAsync(s => s.TemplateId == t.Id)).Count();
                var instancesCount = (await _uow.WorkflowInstances.ListAsync(i => i.TemplateId == t.Id)).Count();
                result.Add(new WorkflowTemplateListItemDto
                {
                    Id = t.Id,
                    NameAr = t.NameAr,
                    NameEn = t.NameEn,
                    CommitteeId = t.CommitteeId,
                    CommitteeName = t.Committee != null
                        ? (language == LanguageDbEnum.Arabic ? t.Committee.NameAr : t.Committee.NameEn)
                        : null,
                    IsActive = t.IsActive,
                    Version = t.Version,
                    StepsCount = stepsCount,
                    InstancesCount = instancesCount
                });
            }
            return result;
        }

        public async Task<WorkflowTemplateDto?> GetTemplateAsync(int id, LanguageDbEnum language)
        {
            var t = await _uow.WorkflowTemplates.GetIncludeAllAsync(id);
            if (t == null) return null;
            return MapTemplate(t, language);
        }

        public async Task<WorkflowTemplateDto> CreateTemplateAsync(WorkflowTemplatePostDto dto, string userId, LanguageDbEnum language)
        {
            if (string.IsNullOrWhiteSpace(dto.NameEn) || string.IsNullOrWhiteSpace(dto.NameAr))
                throw new ArgumentException(MessageConstants.ErrorOccured);

            var template = new WorkflowTemplate
            {
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                CommitteeId = dto.CommitteeId,
                Version = 1,
                IsActive = dto.IsActive,
                InitiatorActorSourceType = dto.InitiatorActorSourceType,
                InitiatorActorTargetId = dto.InitiatorActorTargetId,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };
            await _uow.WorkflowTemplates.AddAsync(template);
            await _uow.SaveChangesAsync();

            var reloaded = await _uow.WorkflowTemplates.GetIncludeAllAsync(template.Id)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
            return MapTemplate(reloaded, language);
        }

        public async Task<WorkflowTemplateDto?> UpdateTemplateAsync(int id, WorkflowTemplatePostDto dto, string userId, LanguageDbEnum language)
        {
            var template = await _uow.WorkflowTemplates.GetAsync(t => t.Id == id);
            if (template == null) return null;

            template.NameAr = dto.NameAr;
            template.NameEn = dto.NameEn;
            template.DescriptionAr = dto.DescriptionAr;
            template.DescriptionEn = dto.DescriptionEn;
            template.CommitteeId = dto.CommitteeId;
            template.IsActive = dto.IsActive;
            template.InitiatorActorSourceType = dto.InitiatorActorSourceType;
            template.InitiatorActorTargetId = dto.InitiatorActorTargetId;
            template.UpdatedBy = userId;
            template.UpdatedDate = DateTime.UtcNow;
            await _uow.SaveChangesAsync();

            var reloaded = await _uow.WorkflowTemplates.GetIncludeAllAsync(id);
            return reloaded == null ? null : MapTemplate(reloaded, language);
        }

        public async Task<bool> DeleteTemplateAsync(int id)
        {
            var template = await _uow.WorkflowTemplates.GetAsync(t => t.Id == id);
            if (template == null) return false;

            // Safety: don't delete a template that has running instances
            var hasInstances = (await _uow.WorkflowInstances.ListAsync(i => i.TemplateId == id)).Any();
            if (hasInstances)
                throw new InvalidOperationException(MessageConstants.ErrorOccured);

            _uow.WorkflowTemplates.Remove(template);
            await _uow.SaveChangesAsync();
            return true;
        }

        // ────────────────────────── Designer: Steps ─────────────────────────

        public async Task<WorkflowStepDto> AddStepAsync(int templateId, WorkflowStepPostDto dto, LanguageDbEnum language)
        {
            var step = new WorkflowStep
            {
                TemplateId = templateId,
                StepOrder = dto.StepOrder,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                IsTerminal = dto.IsTerminal,
                IsAutoAdvance = dto.IsAutoAdvance,
                ActorSourceType = dto.ActorSourceType,
                ActorTargetId = dto.ActorTargetId,
                TaskTitleAr = dto.TaskTitleAr,
                TaskTitleEn = dto.TaskTitleEn,
                TaskBodyAr = dto.TaskBodyAr,
                TaskBodyEn = dto.TaskBodyEn,
                SlaDays = dto.SlaDays,
                LegacyBidStatusId = dto.LegacyBidStatusId
            };
            await _uow.WorkflowSteps.AddAsync(step);
            await _uow.SaveChangesAsync();
            return MapStep(step, language);
        }

        public async Task<WorkflowStepDto?> UpdateStepAsync(int stepId, WorkflowStepPostDto dto, LanguageDbEnum language)
        {
            var step = await _uow.WorkflowSteps.GetAsync(s => s.Id == stepId);
            if (step == null) return null;

            step.StepOrder = dto.StepOrder;
            step.NameAr = dto.NameAr;
            step.NameEn = dto.NameEn;
            step.IsTerminal = dto.IsTerminal;
            step.IsAutoAdvance = dto.IsAutoAdvance;
            step.ActorSourceType = dto.ActorSourceType;
            step.ActorTargetId = dto.ActorTargetId;
            step.TaskTitleAr = dto.TaskTitleAr;
            step.TaskTitleEn = dto.TaskTitleEn;
            step.TaskBodyAr = dto.TaskBodyAr;
            step.TaskBodyEn = dto.TaskBodyEn;
            step.SlaDays = dto.SlaDays;
            step.LegacyBidStatusId = dto.LegacyBidStatusId;
            await _uow.SaveChangesAsync();
            return MapStep(step, language);
        }

        public async Task<bool> DeleteStepAsync(int stepId)
        {
            var step = await _uow.WorkflowSteps.GetAsync(s => s.Id == stepId);
            if (step == null) return false;

            // Safety: a step used as current-step by any instance can't be deleted
            var inUse = (await _uow.WorkflowInstances.ListAsync(i => i.CurrentStepId == stepId)).Any();
            if (inUse) throw new InvalidOperationException(MessageConstants.ErrorOccured);

            _uow.WorkflowSteps.Remove(step);
            await _uow.SaveChangesAsync();
            return true;
        }

        // ────────────────────────── Designer: Transitions ─────────────────────────

        public async Task<WorkflowTransitionDto> AddTransitionAsync(int templateId, WorkflowTransitionPostDto dto, LanguageDbEnum language)
        {
            var t = new WorkflowTransition
            {
                TemplateId = templateId,
                FromStepId = dto.FromStepId,
                ToStepId = dto.ToStepId,
                LabelAr = dto.LabelAr,
                LabelEn = dto.LabelEn,
                ActionType = dto.ActionType,
                DisplayOrder = dto.DisplayOrder
            };
            await _uow.WorkflowTransitions.AddAsync(t);
            await _uow.SaveChangesAsync();
            return MapTransition(t, language);
        }

        public async Task<WorkflowTransitionDto?> UpdateTransitionAsync(int transitionId, WorkflowTransitionPostDto dto, LanguageDbEnum language)
        {
            var t = await _uow.WorkflowTransitions.GetAsync(x => x.Id == transitionId);
            if (t == null) return null;

            t.FromStepId = dto.FromStepId;
            t.ToStepId = dto.ToStepId;
            t.LabelAr = dto.LabelAr;
            t.LabelEn = dto.LabelEn;
            t.ActionType = dto.ActionType;
            t.DisplayOrder = dto.DisplayOrder;
            await _uow.SaveChangesAsync();
            return MapTransition(t, language);
        }

        public async Task<bool> DeleteTransitionAsync(int transitionId)
        {
            var t = await _uow.WorkflowTransitions.GetAsync(x => x.Id == transitionId);
            if (t == null) return false;
            _uow.WorkflowTransitions.Remove(t);
            await _uow.SaveChangesAsync();
            return true;
        }

        // ────────────────────────── Runtime: Instance ─────────────────────────

        public async Task<WorkflowInstanceDto?> GetInstanceForBidAsync(int bidId, string currentUserId, LanguageDbEnum language)
        {
            var instance = await _uow.WorkflowInstances.GetByBidAsync(bidId);
            if (instance == null) return null;

            var bid = await _uow.Bids.GetIncludeAllAsync(bidId);
            var step = instance.Template.Steps.First(s => s.Id == instance.CurrentStepId);

            var outgoing = instance.Template.Transitions
                .Where(t => t.FromStepId == instance.CurrentStepId)
                .OrderBy(t => t.DisplayOrder)
                .Select(t => MapTransition(t, language))
                .ToList();

            var actorIds = bid == null ? new HashSet<string>() : await _engine.ResolveActorUserIdsAsync(step, bid);

            return new WorkflowInstanceDto
            {
                Id = instance.Id,
                TemplateId = instance.TemplateId,
                TemplateName = language == LanguageDbEnum.Arabic ? instance.Template.NameAr : instance.Template.NameEn,
                BidId = instance.BidId,
                CurrentStepId = instance.CurrentStepId,
                CurrentStepName = language == LanguageDbEnum.Arabic ? step.NameAr : step.NameEn,
                CurrentStepOrder = step.StepOrder,
                CurrentStepIsTerminal = step.IsTerminal,
                StartedDate = instance.StartedDate,
                CompletedDate = instance.CompletedDate,
                AvailableTransitions = outgoing,
                CanCurrentUserAct = actorIds.Contains(currentUserId)
            };
        }

        public async Task<List<WorkflowHistoryItemDto>> GetHistoryAsync(int instanceId, LanguageDbEnum language)
        {
            var history = await _uow.WorkflowHistory.ListForInstanceAsync(instanceId);
            return history.Select(h => new WorkflowHistoryItemDto
            {
                Id = h.Id,
                FromStepId = h.FromStepId,
                FromStepName = h.FromStep == null ? null : (language == LanguageDbEnum.Arabic ? h.FromStep.NameAr : h.FromStep.NameEn),
                ToStepId = h.ToStepId,
                ToStepName = language == LanguageDbEnum.Arabic ? h.ToStep.NameAr : h.ToStep.NameEn,
                TransitionId = h.TransitionId,
                TransitionLabel = h.Transition == null ? null : (language == LanguageDbEnum.Arabic ? h.Transition.LabelAr : h.Transition.LabelEn),
                ChangedBy = h.ChangedBy,
                ChangedByName = h.ChangedByNavigation == null ? string.Empty : (language == LanguageDbEnum.Arabic ? h.ChangedByNavigation.FullnameAr : h.ChangedByNavigation.FullnameEn),
                ChangedDate = h.ChangedDate,
                Note = h.Note
            }).ToList();
        }

        // ────────────────────────── Runtime: Tasks ─────────────────────────

        public async Task<List<WorkflowTaskDto>> ListMyTasksAsync(string userId, bool includeCompleted, LanguageDbEnum language)
        {
            var tasks = (await _uow.WorkflowTasks.ListForUserAsync(userId, includeCompleted)).ToList();
            var now = DateTime.UtcNow;
            return tasks.Select(t => MapTask(t, language, now)).ToList();
        }

        public async Task<WorkflowTask?> FireTransitionAsync(int instanceId, FireTransitionDto dto, string userId)
        {
            await _engine.FireTransitionAsync(instanceId, dto.TransitionId, userId, dto.Note, dto.LinkedMeetingId);
            return null;
        }

        // ────────────────────────── Actor options (designer dropdown) ─────────────────────────

        public async Task<ActorOptionsDto> GetActorOptionsAsync()
        {
            var roles = (await _uow.Roles.ListAsync()).ToList();
            var groups = (await _uow.Groups.ListAsync()).ToList();
            var committeeRoles = (await _uow.CommitteeRoles.ListAsync()).ToList();
            var permissions = (await _uow.Permissions.ListAsync()).ToList();

            return new ActorOptionsDto
            {
                Roles = roles.Select(r => new ActorTargetDto { Id = r.Id.ToString(), LabelAr = r.RoleNameAr, LabelEn = r.RoleNameEn }).ToList(),
                Groups = groups.Select(g => new ActorTargetDto { Id = g.Id.ToString(), LabelAr = g.NameAr, LabelEn = g.NameEn }).ToList(),
                CommitteeRoles = committeeRoles.Select(cr => new ActorTargetDto { Id = cr.Id.ToString(), LabelAr = cr.NameAr, LabelEn = cr.NameEn }).ToList(),
                Permissions = permissions.Select(p => new ActorTargetDto { Id = p.Id.ToString(), LabelAr = p.Name, LabelEn = p.Name }).ToList()
            };
        }

        // ────────────────────────── Mappers ─────────────────────────

        private WorkflowTemplateDto MapTemplate(WorkflowTemplate t, LanguageDbEnum language)
        {
            return new WorkflowTemplateDto
            {
                Id = t.Id,
                NameAr = t.NameAr,
                NameEn = t.NameEn,
                DescriptionAr = t.DescriptionAr,
                DescriptionEn = t.DescriptionEn,
                CommitteeId = t.CommitteeId,
                CommitteeName = t.Committee != null ? (language == LanguageDbEnum.Arabic ? t.Committee.NameAr : t.Committee.NameEn) : null,
                Version = t.Version,
                IsActive = t.IsActive,
                InitiatorActorSourceType = t.InitiatorActorSourceType,
                InitiatorActorTargetId = t.InitiatorActorTargetId,
                CreatedDate = t.CreatedDate,
                Steps = t.Steps.OrderBy(s => s.StepOrder).Select(s => MapStep(s, language)).ToList(),
                Transitions = t.Transitions.Select(tr => MapTransition(tr, language)).ToList()
            };
        }

        private static WorkflowStepDto MapStep(WorkflowStep s, LanguageDbEnum language)
        {
            return new WorkflowStepDto
            {
                Id = s.Id,
                TemplateId = s.TemplateId,
                StepOrder = s.StepOrder,
                NameAr = s.NameAr,
                NameEn = s.NameEn,
                IsTerminal = s.IsTerminal,
                IsAutoAdvance = s.IsAutoAdvance,
                ActorSourceType = s.ActorSourceType,
                ActorSourceTypeName = ((ActorSourceTypeDbEnum)s.ActorSourceType).ToString(),
                ActorTargetId = s.ActorTargetId,
                TaskTitleAr = s.TaskTitleAr,
                TaskTitleEn = s.TaskTitleEn,
                TaskBodyAr = s.TaskBodyAr,
                TaskBodyEn = s.TaskBodyEn,
                SlaDays = s.SlaDays,
                LegacyBidStatusId = s.LegacyBidStatusId
            };
        }

        private static WorkflowTransitionDto MapTransition(WorkflowTransition t, LanguageDbEnum language)
        {
            return new WorkflowTransitionDto
            {
                Id = t.Id,
                TemplateId = t.TemplateId,
                FromStepId = t.FromStepId,
                ToStepId = t.ToStepId,
                LabelAr = t.LabelAr,
                LabelEn = t.LabelEn,
                ActionType = t.ActionType,
                ActionTypeName = ((WorkflowActionTypeDbEnum)t.ActionType).ToString(),
                DisplayOrder = t.DisplayOrder
            };
        }

        private static WorkflowTaskDto MapTask(WorkflowTask task, LanguageDbEnum language, DateTime now)
        {
            var step = task.Step;
            var bid = task.Instance?.Bid;
            var isDelayed = task.DueDate.HasValue
                && task.DueDate.Value < now
                && task.StatusId != (int)WorkflowTaskStatusDbEnum.Completed
                && task.StatusId != (int)WorkflowTaskStatusDbEnum.Cancelled;

            return new WorkflowTaskDto
            {
                Id = task.Id,
                InstanceId = task.InstanceId,
                BidId = task.BidId,
                BidReferenceNumber = bid?.ReferenceNumber,
                BidSubject = bid?.Subject,
                CommitteeName = bid?.Committee == null ? null : (language == LanguageDbEnum.Arabic ? bid.Committee.NameAr : bid.Committee.NameEn),
                StepId = task.StepId,
                StepName = step == null ? string.Empty : (language == LanguageDbEnum.Arabic ? step.NameAr : step.NameEn),
                TaskTitle = step == null ? null : (language == LanguageDbEnum.Arabic ? step.TaskTitleAr : step.TaskTitleEn),
                TaskBody = step == null ? null : (language == LanguageDbEnum.Arabic ? step.TaskBodyAr : step.TaskBodyEn),
                AssignedToUserId = task.AssignedToUserId,
                StatusId = task.StatusId,
                StatusName = ((WorkflowTaskStatusDbEnum)task.StatusId).ToString(),
                DueDate = task.DueDate,
                ClaimedDate = task.ClaimedDate,
                CompletedDate = task.CompletedDate,
                CreatedDate = task.CreatedDate,
                IsDelayed = isDelayed
            };
        }
    }
}
