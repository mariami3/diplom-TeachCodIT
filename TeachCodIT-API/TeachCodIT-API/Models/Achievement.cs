using System;
using System.Collections.Generic;

namespace TeachCodIT_API.Models;

public partial class Achievement
{
    public int IdAchievement { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public int? Xpreward { get; set; }

    public string? Icon { get; set; }

    public string? Type { get; set; }

    public int? TargetValue { get; set; }

    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
