using System;
using System.Collections.Generic;

namespace TeachCodIT.Models;

public partial class DailyQuest
{
    public int IdQuest { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public int? Xpreward { get; set; }

    public string? Type { get; set; }

    public int? TargetValue { get; set; }

    public int? IncrementStep { get; set; }

    public virtual ICollection<UserDailyQuest> UserDailyQuests { get; set; } = new List<UserDailyQuest>();
}
