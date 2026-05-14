namespace TeachCodIT.Models.ViewModels
{
    public class AdminSettingsViewModel
    {
        public string Theme { get; set; } = "light";
        public string Language { get; set; } = "ru";
        public string TimeZone { get; set; } = "Europe/Moscow";

        public User? User { get; set; }
        public UserProfile? Profile { get; set; }
        public UserProgress? Progress { get; set; }
    }
}