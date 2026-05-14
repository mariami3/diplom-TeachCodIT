using TeachCodIT.Models.ViewModels;

namespace TeachCodIT.Models.ViewModels
{
    public class TeacherAnalyticsViewModel
    {
        public int AverageProgress { get; set; }

        public List<StudentCourseViewModel> TopStudents { get; set; } = new();

        public List<StudentCourseViewModel> LowStudents { get; set; } = new();
    }
}
