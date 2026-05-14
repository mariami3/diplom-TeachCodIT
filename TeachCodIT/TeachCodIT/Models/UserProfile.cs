using System;
using System.Collections.Generic;

namespace TeachCodIT.Models;

public partial class UserProfile
{
    public int IdProfile { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public int? UserId { get; set; }

    public virtual User? User { get; set; }
}
