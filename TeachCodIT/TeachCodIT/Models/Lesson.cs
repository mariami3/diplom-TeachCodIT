using System;
using System.Collections.Generic;

namespace TeachCodIT.Models;

public partial class Lesson
{
    public int IdLesson { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public int? ModuleId { get; set; }

    public int OrderIndex { get; set; }

    public int? Xpreward { get; set; }

    public virtual ICollection<LessonTask> LessonTasks { get; set; } = new List<LessonTask>();

    public virtual Module? Module { get; set; }

    public virtual ICollection<UserLessonCompletion> UserLessonCompletions { get; set; } = new List<UserLessonCompletion>();
}
