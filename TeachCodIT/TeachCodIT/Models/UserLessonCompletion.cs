using System;
using System.Collections.Generic;

namespace TeachCodIT.Models;

public partial class UserLessonCompletion
{
    public int IdUserLessonCompletion { get; set; }

    public int UserId { get; set; }

    public int LessonId { get; set; }

    public DateTime CompletedAt { get; set; }

    public int EarnedXp { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
