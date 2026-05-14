using TeachCodIT.Models;

namespace TeachCodIT.Models.ViewModels
{
    public class StudentCourseViewModel
    {
        public int StudentCourseId { get; set; }

        public int CourseId { get; set; }

        public string CourseTitle { get; set; }

        public string CourseDescription { get; set; }

        public int OverallProgressPercent { get; set; }

        public int ProgressPercent { get; set; }

        public DateTime EnrolledAt { get; set; }

        public string GradientColor { get; set; }

        public int StudentId { get; set; }

        public string FullName { get; set; }

        public int TotalXP { get; set; }

        public int StreakDays { get; set; }

        public User User { get; set; }

        public DateTime? LastActivityDate { get; set; }

        public int ModulesCount { get; set; }
        public int LessonsCompleted { get; set; }
        public int TotalLessons { get; set; }
    }
}