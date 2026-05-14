namespace TeachCodIT.Models.ViewModels
{
    using TeachCodIT.Models;

    public class LessonTaskViewModel
    {
        public int IdLessonTask { get; set; }
        public string Title { get; set; }
        public string TaskType { get; set; }
        public string Description { get; set; }
        public int XPReward { get; set; }
        public DateTime? Deadline { get; set; }
        public List<TaskOptionViewModel> Options { get; set; } = new();
        public UserTaskAttempt? LastAttempt { get; set; }
        public AttemptInfo? LastAttemptSummary { get; set; }
        public bool IsSolvedCorrectly { get; set; }
        public string? ExampleCode { get; set; }
        public string? UserCode { get; set; }
        public bool IsCompleted { get; set; }
        public int AttemptsCount { get; set; }
        public string DisplayType => TaskType switch
        {
            "test" => "multiple-choice",
            "code" => "code-editor",
            _ => "default"
        };
    }

    public class AttemptInfo
    {
        public bool? IsCorrect { get; set; }
        public DateTime? AttemptDate { get; set; }
        public string SubmittedAnswer { get; set; }
    }
}
