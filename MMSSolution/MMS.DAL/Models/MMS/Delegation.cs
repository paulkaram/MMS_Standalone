namespace MMS.DAL.Models.MMS;

public partial class Delegation
{
    public int Id { get; set; }

    public string FromUserId { get; set; } = null!;

    public string ToUserId { get; set; } = null!;

    public int TypeId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public virtual User FromUser { get; set; } = null!;

    public virtual User ToUser { get; set; } = null!;

    public virtual ICollection<DelegationTask> DelegationTasks { get; set; } = new List<DelegationTask>();
}
