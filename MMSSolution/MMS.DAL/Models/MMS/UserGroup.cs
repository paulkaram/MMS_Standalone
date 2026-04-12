namespace MMS.DAL.Models.MMS;

public class UserGroup
{
    public string UserId { get; set; } = null!;

    public int GroupId { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Group Group { get; set; } = null!;
}
