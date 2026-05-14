using System;
using System.Collections.Generic;

namespace TeachCodIT_API.Models;

public partial class Role
{
    public int IdRole { get; set; }

    public string NameRole { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
