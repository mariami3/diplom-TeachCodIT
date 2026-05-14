using System;
using System.Collections.Generic;

namespace TeachCodIT.Models;

public partial class UserDailyQuest
{
    public int IdUserDailyQuest { get; set; }

    public int? UserId { get; set; }

    public int? QuestId { get; set; }

    public DateOnly QuestDate { get; set; }

    public bool? IsCompleted { get; set; }

    public int? CurrentValue { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int? TargetValue { get; set; }

    public int? ProgressPercent { get; set; }

    public virtual DailyQuest? Quest { get; set; }

    public virtual User? User { get; set; }
}
