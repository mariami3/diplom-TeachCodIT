using System;
using System.Collections.Generic;

namespace TeachCodIT_API.Models;

public partial class UserTaskAttempt
{
    public int IdAttempt { get; set; }

    public int? UserId { get; set; }

    public int? LessonTaskId { get; set; }

    public string? SubmittedAnswer { get; set; }

    public bool? IsCorrect { get; set; }

    public int? EarnedXp { get; set; }

    public DateTime? AttemptDate { get; set; }

    public string? Comment { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public int? AttemptNumber { get; set; }

    public virtual LessonTask? LessonTask { get; set; }

    public virtual User? User { get; set; }
}
