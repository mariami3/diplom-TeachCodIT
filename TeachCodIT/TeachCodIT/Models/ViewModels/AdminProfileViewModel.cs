using TeachCodIT.Models;

namespace TeachCodIT.Models.ViewModels
{
    public class AdminProfileViewModel
    {
        public User? User { get; set; }
        public UserProfile? Profile { get; set; }
        public UserProgress? Progress { get; set; }

        public DateTime RegistrationDate { get; set; }

        // Статистика платформы
        public int TotalUsers { get; set; } = 0;
        public int TotalCourses { get; set; } = 0;
        public int TotalAchievements { get; set; } = 0;
        public int ActiveUsers { get; set; } = 0;
        public int PublishedCourses { get; set; } = 0;
        public int TotalQuests { get; set; } = 0;

        public DateTime? LastLogin { get; set; }
    }
}