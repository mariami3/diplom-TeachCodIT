namespace TeachCodIT.Models.ViewModels
{
    public class DailyLoginViewModel
    {
        public bool AlreadyLogged { get; set; }
        public bool Success { get; set; }

        public int StreakDays { get; set; }
        public int EarnedXP { get; set; }

        public UserProgress Progress { get; set; }
        public User User { get; set; }
        public UserProfile Profile { get; set; }
    }
}
