namespace MMS.DAL.Models.MMS;

public class UserRole
{
    public string UserId { get; set; } = null!;

    public int RoleId { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
