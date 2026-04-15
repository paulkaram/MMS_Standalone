namespace MMS.DAL.Enumerations
{
    /// <summary>
    /// Determines HOW a workflow step resolves its assignees at runtime.
    /// All values are pure RBAC references — no role is hardcoded in C#.
    /// Role / Group / CommitteeRole / Permission / SpecificUser read their
    /// target ID from WorkflowStep.ActorTargetId. Stakeholder / TeamLeader /
    /// Creator read directly from the Bid instance.
    /// </summary>
    public enum ActorSourceTypeDbEnum
    {
        Role = 1,
        Group = 2,
        CommitteeRole = 3,
        Permission = 4,
        Stakeholder = 5,
        TeamLeader = 6,
        Creator = 7,
        SpecificUser = 8
    }

    /// <summary>
    /// Classifies a transition's visual treatment and semantic intent.
    /// Approve/Reject are hints for the UI (green tick / red X button);
    /// Auto transitions are fired by the engine (e.g., all visions submitted).
    /// </summary>
    public enum WorkflowActionTypeDbEnum
    {
        Advance = 1,
        Approve = 2,
        Reject = 3,
        Auto = 4
    }

    public enum WorkflowTaskStatusDbEnum
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4
    }
}
