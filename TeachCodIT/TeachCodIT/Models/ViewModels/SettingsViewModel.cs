namespace TeachCodIT.Models.ViewModels
{
    public class SettingsViewModel
    {
        public string Theme { get; set; }
        public string Language { get; set; }
        public string TimeZone { get; set; }

        public User User { get; set; }
        public UserProfile Profile { get; set; }
        public UserProgress Progress { get; set; }
    }
}
