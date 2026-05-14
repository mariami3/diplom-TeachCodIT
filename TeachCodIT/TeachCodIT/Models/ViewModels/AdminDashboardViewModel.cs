namespace TeachCodIT.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }

        public List<TopStudent> TopStudents { get; set; } = new();

        public List<PopularCourse> PopularCourses { get; set; } = new();

        public string AverageLearningTime { get; set; } = "0 часов";

        public List<string> RegistrationChartLabels { get; set; } = new();
        public List<int> RegistrationChartData { get; set; } = new();

        public List<string> CourseCompletionLabels { get; set; } = new();
        public List<int> CourseCompletionData { get; set; } = new();

        public int MaxXP { get; set; }
        public int ActiveStudentsCount => TopStudents?.Count(s => s.XP > 0) ?? 0;
    }

    public class TopStudent
    {
        public int UserId { get; set; }
        public string Login { get; set; } = string.Empty;
        public int XP { get; set; }
        public int Level { get; set; }
        public int StreakDays { get; set; }
    }

    public class PopularCourse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Enrollments { get; set; }
        public int AverageProgress { get; set; }   // в процентах
        public string? GradientColor { get; set; }
    }
}
