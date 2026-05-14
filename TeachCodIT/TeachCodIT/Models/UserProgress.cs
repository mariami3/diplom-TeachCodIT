using System;
using System.Collections.Generic;

namespace TeachCodIT.Models;

public partial class UserProgress
{
    public int UserId { get; set; }

    public int? Xp { get; set; }

    public int? Level { get; set; }

    public int? StreakDays { get; set; }

    public DateOnly? LastActivityDate { get; set; }

    public virtual User User { get; set; } = null!;
}
