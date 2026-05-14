using TeachCodIT.Models;

namespace TeachCodIT.Models.ViewModels
{
    public class TeacherProfileViewModel
    {
        public User? User { get; set; }
        public UserProfile? Profile { get; set; }
        public UserProgress? Progress { get; set; }

        public DateTime RegistrationDate { get; set; }

        public int TotalCourses { get; set; } = 0;
        public int PublishedCourses { get; set; } = 0;
        public int TotalStudents { get; set; } = 0;
        public int ActiveStudents { get; set; } = 0;
        public int TotalTasks { get; set; } = 0;
        public int PendingReviews { get; set; } = 0;

        public DateTime? LastLogin { get; set; }
    }
}