namespace TeachCodIT.Models.ViewModels
{
    public class StudentDashboardViewModel
    {
        public User User { get; set; }
        public UserProfile Profile { get; set; }
        public UserProgress Progress { get; set; }
        public List<StudentCourseViewModel> StudentCourses { get; set; } = new();
        public List<UserAchievementViewModel> UserAchievements { get; set; } = new();
        public List<UserDailyQuestViewModel> DailyQuests { get; set; } = new();
        public List<LeaderboardEntry> Leaderboard { get; set; } = new();
        public List<WeakTopicViewModel> WeakTopics { get; set; } = new();
        public int TotalCourses { get; set; }

        // Вычисляемые свойства
        public string FullName => Profile != null
            ? $"{Profile.FirstName} {Profile.LastName}"
            : User?.LoginUser ?? "Студент";

        public string FirstName => Profile?.FirstName ?? User?.LoginUser ?? "Студент";

        public int TotalXP => Progress?.Xp ?? 0;
        public int CurrentLevel => Progress?.Level ?? 1;
        public int StreakDays => Progress?.StreakDays ?? 0;

        public int CompletedCourses => StudentCourses?.Count(c => c.ProgressPercent >= 100) ?? 0;
        public int InProgressCourses => StudentCourses?.Count(c => c.ProgressPercent > 0 && c.ProgressPercent < 100) ?? 0;

        public int XpToNextLevel
        {
            get
            {
                int currentLevel = CurrentLevel;
                int xpForCurrentLevel = currentLevel * 100;
                int xpForNextLevel = (currentLevel + 1) * 100;
                return xpForNextLevel - TotalXP;
            }
        }

        public int NextLevel => CurrentLevel + 1;

        public double OverallProgressPercent
        {
            get
            {
                if (StudentCourses == null || !StudentCourses.Any())
                    return 0;
                return StudentCourses.Average(c => c.ProgressPercent);
            }
        }

        public int DailyQuestsCompleted => DailyQuests?.Count(q => q.IsCompleted) ?? 0;
        public int DailyQuestsTotal => DailyQuests?.Count ?? 0;
    }



    public class UserAchievementViewModel
    {
        public int UserAchievementId { get; set; }
        public int AchievementId { get; set; }
        public string AchievementTitle { get; set; }
        public string AchievementDescription { get; set; }
        public int XPReward { get; set; }
        public DateTime EarnedAt { get; set; }
        public string Icon { get; set; }
        public bool IsUnlocked { get; set; }

    }

    public class UserDailyQuestViewModel
    {
        public int UserDailyQuestId { get; set; }
        public int QuestId { get; set; }
        public string QuestTitle { get; set; } = string.Empty;
        public string QuestDescription { get; set; } = string.Empty;
        public int XPReward { get; set; }
        public DateTime QuestDate { get; set; }
        public bool IsCompleted { get; set; }

        // Новое свойство
        public bool IsAutoCompleted { get; set; }
        public int ProgressPercent { get; set; }
        public int CurrentValue { get; set; }
        public int TargetValue { get; set; }
        public string DynamicTitle { get; set; }
        public DateTime CompletedAt { get; set; }
    }

    public class LeaderboardEntry
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public int XP { get; set; }
        public int Level { get; set; }
        public int StreakDays { get; set; }
        public int Rank { get; set; }
        public bool IsCurrentUser { get; set; }
    }

    public class ProfileViewModel
    {
        public User User { get; set; }
        public UserProfile Profile { get; set; }
        public UserProgress Progress { get; set; }
        public DateTime RegistrationDate { get; set; }

        // Статистика
        public int CompletedCourses { get; set; }
        public int TotalCourses { get; set; }
        public int CompletedLessons { get; set; }
        public int CompletedTasks { get; set; }
        public int AchievementsCount { get; set; }

        public List<UserAchievementViewModel>? RecentAchievements { get; set; }

        public List<ActivityItem>? RecentActivity { get; set; }
    }

    public class ActivityItem
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public int EarnedXP { get; set; }
        public string Type { get; set; } = string.Empty; 
    }
}