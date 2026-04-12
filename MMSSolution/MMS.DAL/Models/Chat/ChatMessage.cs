using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.Chat;

public partial class ChatMessage
{
    public int Id { get; set; }

    public int ChatId { get; set; }

    public string UserId { get; set; } = null!;

    public string MessageText { get; set; } = null!;

    public DateTime SentAt { get; set; }

    public virtual Chat Chat { get; set; } = null!;

    public virtual ICollection<ChatMessageContent> ChatMessageContents { get; set; } = new List<ChatMessageContent>();
}
