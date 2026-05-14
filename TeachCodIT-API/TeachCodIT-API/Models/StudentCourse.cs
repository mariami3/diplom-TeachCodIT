using System;
using System.Collections.Generic;

namespace TeachCodIT_API.Models;

public partial class StudentCourse
{
    public int IdStudentCourse { get; set; }

    public int? StudentId { get; set; }

    public int? CourseId { get; set; }

    public DateTime? EnrolledAt { get; set; }

    public int? ProgressPercent { get; set; }

    public virtual Course? Course { get; set; }

    public virtual User? Student { get; set; }
}
