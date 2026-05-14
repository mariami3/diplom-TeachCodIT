namespace TeachCodIT.Models.ViewModels
{
    public class LessonViewModel
    {
        public int IdLesson { get; set; }
        public int? CourseId => Module?.CourseId;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public int XPReward => Lesson.Xpreward ?? 0;
        public Lesson Lesson { get; set; } = new();

        public Module? Module { get; set; }
        public Course? Course { get; set; }

        public bool IsCompleted { get; set; }
        public int EarnedXpFromLesson { get; set; }

    
        public List<LessonTaskViewModel> Tasks { get; set; } = new();

        public int CompletedTasksCount { get; set; }
        public int TotalTasksCount { get; set; }
    }
}