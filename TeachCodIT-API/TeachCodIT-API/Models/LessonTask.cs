using System;
using System.Collections.Generic;

namespace TeachCodIT_API.Models;

public partial class LessonTask
{
    public int IdLessonTask { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? TaskType { get; set; }

    public int? LessonId { get; set; }

    public int? Xpreward { get; set; }

    public DateTime? Deadline { get; set; }

    public string? ExampleCode { get; set; }

    public int? MaxAttempts { get; set; }

    public string? ExpectedOutput { get; set; }

    public string? CheckerType { get; set; }

    public string? StarterCode { get; set; }

    public string? TestInput { get; set; }

    public virtual Lesson? Lesson { get; set; }

    public virtual ICollection<TaskOption> TaskOptions { get; set; } = new List<TaskOption>();

    public virtual ICollection<UserTaskAttempt> UserTaskAttempts { get; set; } = new List<UserTaskAttempt>();
}
