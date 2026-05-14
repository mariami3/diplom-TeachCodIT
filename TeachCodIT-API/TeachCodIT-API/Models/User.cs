using System;
using System.Collections.Generic;

namespace TeachCodIT_API.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string LoginUser { get; set; } = null!;

    public string PasswordUser { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiry { get; set; }

    public int? RoleId { get; set; }

    public DateTime RegistrationDate { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();

    public virtual ICollection<UserDailyLogin> UserDailyLogins { get; set; } = new List<UserDailyLogin>();

    public virtual ICollection<UserDailyQuest> UserDailyQuests { get; set; } = new List<UserDailyQuest>();

    public virtual ICollection<UserLessonCompletion> UserLessonCompletions { get; set; } = new List<UserLessonCompletion>();

    public virtual ICollection<UserProfile> UserProfiles { get; set; } = new List<UserProfile>();

    public virtual UserProgress? UserProgress { get; set; }

    public virtual UserSetting? UserSetting { get; set; }

    public virtual ICollection<UserTaskAttempt> UserTaskAttempts { get; set; } = new List<UserTaskAttempt>();
}
