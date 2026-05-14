using System;
using System.Collections.Generic;

namespace TeachCodIT_API.Models;

public partial class UserDailyLogin
{
    public int IdDailyLogin { get; set; }

    public int UserId { get; set; }

    public DateOnly LoginDate { get; set; }

    public int EarnedXp { get; set; }

    public virtual User User { get; set; } = null!;
}
