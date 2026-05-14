using System;
using System.Collections.Generic;

namespace TeachCodIT.Models;

public partial class Module
{
    public int IdModule { get; set; }

    public string Title { get; set; } = null!;

    public int? CourseId { get; set; }

    public int OrderIndex { get; set; }

    public virtual Course? Course { get; set; }

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
