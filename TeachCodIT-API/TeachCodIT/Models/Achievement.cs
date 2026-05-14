using System;
using System.Collections.Generic;

namespace TeachCodIT.Models;

public partial class Achievement
{
    public int IdAchievement { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public int? Xpreward { get; set; }

    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
