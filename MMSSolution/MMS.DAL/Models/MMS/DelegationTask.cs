namespace MMS.DAL.Models.MMS;

public partial class DelegationTask
{
    public int Id { get; set; }

    public int DelegationId { get; set; }

    public int TaskId { get; set; }

    public virtual Delegation Delegation { get; set; } = null!;

    public virtual Task TaskNavigation { get; set; } = null!;
}
