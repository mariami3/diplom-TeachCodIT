using TeachCodIT.Models.ViewModels;

namespace TeachCodIT.Models.ViewModels
{
    public class TeacherAnalyticsViewModel
    {
        public int AverageProgress { get; set; }

        public int TotalStudents { get; set; }
        public int CompletedStudents { get; set; }

        public List<TaskAnalyticsViewModel> Tasks { get; set; }
    }
    public class TaskAnalyticsViewModel
    {
        public string Title { get; set; }
        public string Type { get; set; }

        public int CompletedCount { get; set; }
        public int TotalStudents { get; set; }

        public double AverageScore { get; set; }

        public DateTime? Deadline { get; set; }
    }

    public class ModuleAnalyticsViewModel
    {
        public int ModuleId { get; set; }
        public string ModuleTitle { get; set; } = string.Empty;
        public List<StudentModuleResultViewModel> Students { get; set; } = new();
    }

    public class StudentModuleResultViewModel
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int TotalXP { get; set; }
        public List<TaskResultViewModel> Tasks { get; set; } = new();
    }

    public class TaskResultViewModel
    {
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string TaskType { get; set; } = string.Empty; 
        public bool? IsCorrect { get; set; }
        public int? EarnedXP { get; set; }
        public DateTime? LastAttempt { get; set; }
    }
}
