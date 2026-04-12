using System;
using System.Collections.Generic;

namespace MMS.DAL.Models.MMS;

public partial class RecommendationNote
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;

    public int RecommendationId { get; set; }

    public DateTime CreatedAt { get; set; }
}
