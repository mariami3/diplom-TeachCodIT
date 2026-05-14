using System;
using System.Collections.Generic;

namespace TeachCodIT.Models;

public partial class Course
{
    public int IdCourse { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? CreatedBy { get; set; }

    public bool IsPublished { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? GradientColor { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Module> Modules { get; set; } = new List<Module>();

    public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
}
