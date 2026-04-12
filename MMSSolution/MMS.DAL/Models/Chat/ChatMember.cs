using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.Chat;

public partial class ChatMember
{
    public int Id { get; set; }

    public int ChatId { get; set; }

    public string UserId { get; set; } = null!;

    public bool IsAdmin { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Nickname { get; set; }

    public int UnreadMessages { get; set; }

    public virtual Chat Chat { get; set; } = null!;
}
