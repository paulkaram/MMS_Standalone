namespace MMS.DTO.Workflow
{
    // ────────────────────────────────────────────────────────────────────
    //  Template / Step / Transition — designer-facing DTOs
    // ────────────────────────────────────────────────────────────────────

    public class WorkflowTemplateDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public int? CommitteeId { get; set; }
        public string? CommitteeName { get; set; }
        public int Version { get; set; }
        public bool IsActive { get; set; }
        public int? InitiatorActorSourceType { get; set; }
        public string? InitiatorActorTargetId { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<WorkflowStepDto> Steps { get; set; } = new();
        public List<WorkflowTransitionDto> Transitions { get; set; } = new();
    }

    public class WorkflowTemplateListItemDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public int? CommitteeId { get; set; }
        public string? CommitteeName { get; set; }
        public bool IsActive { get; set; }
        public int Version { get; set; }
        public int StepsCount { get; set; }
        public int InstancesCount { get; set; }
    }

    public class WorkflowTemplatePostDto
    {
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public int? CommitteeId { get; set; }
        public bool IsActive { get; set; } = true;
        public int? InitiatorActorSourceType { get; set; }
        public string? InitiatorActorTargetId { get; set; }
    }

    public class WorkflowStepDto
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public int StepOrder { get; set; }
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public bool IsTerminal { get; set; }
        public bool IsAutoAdvance { get; set; }
        public int ActorSourceType { get; set; }
        public string ActorSourceTypeName { get; set; } = null!;
        public string? ActorTargetId { get; set; }
        public string? ActorTargetLabel { get; set; }
        public string? TaskTitleAr { get; set; }
        public string? TaskTitleEn { get; set; }
        public string? TaskBodyAr { get; set; }
        public string? TaskBodyEn { get; set; }
        public int? SlaDays { get; set; }
        public int? LegacyBidStatusId { get; set; }
    }

    public class WorkflowStepPostDto
    {
        public int StepOrder { get; set; }
        public string NameAr { get; set; } = null!;
        public string NameEn { get; set; } = null!;
        public bool IsTerminal { get; set; }
        public bool IsAutoAdvance { get; set; }
        public int ActorSourceType { get; set; }
        public string? ActorTargetId { get; set; }
        public string? TaskTitleAr { get; set; }
        public string? TaskTitleEn { get; set; }
        public string? TaskBodyAr { get; set; }
        public string? TaskBodyEn { get; set; }
        public int? SlaDays { get; set; }
        public int? LegacyBidStatusId { get; set; }
    }

    public class WorkflowTransitionDto
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public int FromStepId { get; set; }
        public int ToStepId { get; set; }
        public string LabelAr { get; set; } = null!;
        public string LabelEn { get; set; } = null!;
        public int ActionType { get; set; }
        public string ActionTypeName { get; set; } = null!;
        public int DisplayOrder { get; set; }
    }

    public class WorkflowTransitionPostDto
    {
        public int FromStepId { get; set; }
        public int ToStepId { get; set; }
        public string LabelAr { get; set; } = null!;
        public string LabelEn { get; set; } = null!;
        public int ActionType { get; set; }
        public int DisplayOrder { get; set; }
    }

    // ────────────────────────────────────────────────────────────────────
    //  Runtime instance / task DTOs
    // ────────────────────────────────────────────────────────────────────

    public class WorkflowInstanceDto
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = null!;
        public int BidId { get; set; }
        public int CurrentStepId { get; set; }
        public string CurrentStepName { get; set; } = null!;
        public int CurrentStepOrder { get; set; }
        public bool CurrentStepIsTerminal { get; set; }
        public DateTime StartedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public List<WorkflowTransitionDto> AvailableTransitions { get; set; } = new();
        public bool CanCurrentUserAct { get; set; }
    }

    public class WorkflowTaskDto
    {
        public int Id { get; set; }
        public int InstanceId { get; set; }
        public int BidId { get; set; }
        public string? BidReferenceNumber { get; set; }
        public string? BidSubject { get; set; }
        public string? CommitteeName { get; set; }
        public int StepId { get; set; }
        public string StepName { get; set; } = null!;
        public string? TaskTitle { get; set; }
        public string? TaskBody { get; set; }
        public string AssignedToUserId { get; set; } = null!;
        public int StatusId { get; set; }
        public string StatusName { get; set; } = null!;
        public DateTime? DueDate { get; set; }
        public DateTime? ClaimedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDelayed { get; set; }
    }

    public class WorkflowHistoryItemDto
    {
        public int Id { get; set; }
        public int? FromStepId { get; set; }
        public string? FromStepName { get; set; }
        public int ToStepId { get; set; }
        public string ToStepName { get; set; } = null!;
        public int? TransitionId { get; set; }
        public string? TransitionLabel { get; set; }
        public string ChangedBy { get; set; } = null!;
        public string ChangedByName { get; set; } = null!;
        public DateTime ChangedDate { get; set; }
        public string? Note { get; set; }
    }

    public class FireTransitionDto
    {
        public int TransitionId { get; set; }
        public string? Note { get; set; }
        /// <summary>
        /// Optional: link a real Meeting to the bid at this transition.
        /// §5.6 steps 5 (Preparatory) and 6 (Ministerial) support attaching a Meeting record.
        /// When provided, Bid.MeetingId is updated on fire.
        /// </summary>
        public int? LinkedMeetingId { get; set; }
    }

    // ────────────────────────────────────────────────────────────────────
    //  Helpers for the designer UI
    // ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returned by GET /api/workflow/actor-options so the designer can
    /// populate the "actor target" picker dynamically based on source type.
    /// </summary>
    public class ActorOptionsDto
    {
        public List<ActorTargetDto> Roles { get; set; } = new();
        public List<ActorTargetDto> Groups { get; set; } = new();
        public List<ActorTargetDto> CommitteeRoles { get; set; } = new();
        public List<ActorTargetDto> Permissions { get; set; } = new();
    }

    public class ActorTargetDto
    {
        public string Id { get; set; } = null!;
        public string LabelAr { get; set; } = null!;
        public string LabelEn { get; set; } = null!;
    }
}
