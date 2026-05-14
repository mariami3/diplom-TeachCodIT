using System;
using System.Collections.Generic;

namespace TeachCodIT.Models;

public partial class TaskOption
{
    public int IdOption { get; set; }

    public int? LessonTaskId { get; set; }

    public string? OptionText { get; set; }

    public bool? IsCorrect { get; set; }

    public virtual LessonTask? LessonTask { get; set; }
}
