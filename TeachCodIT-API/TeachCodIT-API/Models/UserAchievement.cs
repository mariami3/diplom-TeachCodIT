using System;
using System.Collections.Generic;

namespace TeachCodIT_API.Models;

public partial class UserAchievement
{
    public int IdUserAchievement { get; set; }

    public int? UserId { get; set; }

    public int? AchievementId { get; set; }

    public DateTime? EarnedAt { get; set; }

    public virtual Achievement? Achievement { get; set; }

    public virtual User? User { get; set; }
}
