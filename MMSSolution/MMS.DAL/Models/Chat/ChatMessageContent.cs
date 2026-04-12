using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.Chat;

public partial class ChatMessageContent
{
    public int Id { get; set; }

    public int ChatMessageId { get; set; }

    public int ChatMessageTypeId { get; set; }

    public string RelativePath { get; set; } = null!;

    public virtual ChatMessage ChatMessage { get; set; } = null!;

    public virtual ChatMessageType ChatMessageType { get; set; } = null!;
}
