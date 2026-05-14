using System;
using System.Collections.Generic;

namespace TeachCodIT.Models;

public partial class UserSetting
{
    public int IdSetting { get; set; }

    public int UserId { get; set; }

    public string? Theme { get; set; }

    public string? Language { get; set; }

    public string? TimeZone { get; set; }

    public virtual User User { get; set; } = null!;
}
