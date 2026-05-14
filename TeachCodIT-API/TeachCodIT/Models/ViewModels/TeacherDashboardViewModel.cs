using TeachCodIT.Models;

namespace TeachCodIT.Models.ViewModels
{
    public class TeacherDashboardViewModel
    {
        public string TeacherName { get; set; }

        public int TotalCourses { get; set; }

        public int TotalStudents { get; set; }

        public int TotalTasks { get; set; }

        public int ActiveStudentsToday { get; set; }

        public int NewStudentsWeek { get; set; }

        public int PendingTasks { get; set; }

        public double AverageProgress { get; set; }

        public List<Course> Courses { get; set; }

        public List<User> TopStudents { get; set; }
    }
}