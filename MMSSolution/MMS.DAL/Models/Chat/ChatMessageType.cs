using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.Chat;

public partial class ChatMessageType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ChatMessageContent> ChatMessageContents { get; set; } = new List<ChatMessageContent>();
}
