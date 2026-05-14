using Microsoft.EntityFrameworkCore;
using TeachCodIT.Models;

namespace TeachCodIT.Services
{
    public class GamificationService
    {
        private readonly TeachCodItContext _context;

        public GamificationService(TeachCodItContext context)
        {
            _context = context;
        }

        // ================= XP =================
        public async Task AddXP(int userId, int xp)
        {
            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (progress == null)
            {
                progress = new UserProgress
                {
                    UserId = userId,
                    Xp = 0,
                    Level = 1,
                    StreakDays = 0
                };
                _context.UserProgresses.Add(progress);
            }

            progress.Xp = (progress.Xp ?? 0) + xp;
            progress.Level = 1 + ((progress.Xp ?? 0) / 100);

            await _context.SaveChangesAsync();
        }

        // ================= DAILY QUESTS =================
        public async Task CheckDailyQuests(int userId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var todayDateTime = DateTime.UtcNow.Date;

            var quests = await _context.UserDailyQuests
                .Include(q => q.Quest)
                .Where(q => q.UserId == userId
                         && q.QuestDate == today
                         && q.IsCompleted != true)
                .ToListAsync();

            foreach (var q in quests)
            {
                bool completed = false;
                string type = q.Quest?.Type?.ToLowerInvariant() ?? "";

                switch (type)
                {
                    case "correct_tasks":
                        var correctTasksToday = await _context.UserTaskAttempts
                            .CountAsync(t => t.UserId == userId
                                          && t.IsCorrect == true
                                          && t.AttemptDate.HasValue
                                          && t.AttemptDate.Value.Date == todayDateTime);
                        completed = correctTasksToday >= (q.Quest?.TargetValue ?? 1);
                        break;

                    case "lessons":
                        var lessonsToday = await _context.UserLessonCompletions
                            .CountAsync(l => l.UserId == userId
                                          && l.CompletedAt.Date == todayDateTime);
                        completed = lessonsToday >= (q.Quest?.TargetValue ?? 1);
                        break;

                    case "modules":
                        var modulesToday = await _context.UserLessonCompletions
                            .Where(l => l.UserId == userId
                                     && l.CompletedAt.Date == todayDateTime
                                     && l.Lesson != null)
                            .Select(l => l.Lesson.ModuleId)
                            .Distinct()
                            .CountAsync();
                        completed = modulesToday >= (q.Quest?.TargetValue ?? 1);
                        break;

                    case "xp":
                        completed = false; // считается в контроллере DailyQuests
                        break;
                }

                if (completed)
                {
                    q.IsCompleted = true;
                    q.CompletedAt = DateTime.UtcNow;

                    var progress = await _context.UserProgresses
                        .FirstOrDefaultAsync(p => p.UserId == userId);

                    if (progress == null)
                    {
                        progress = new UserProgress
                        {
                            UserId = userId,
                            Xp = 0,
                            Level = 1,
                            StreakDays = 0
                        };
                        _context.UserProgresses.Add(progress);
                    }

                    int reward = q.Quest?.Xpreward ?? 20;
                    progress.Xp = (progress.Xp ?? 0) + reward;
                    progress.Level = 1 + ((progress.Xp ?? 0) / 100);

                    // Streak
                    var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);
                    if (progress.LastActivityDate == todayDate.AddDays(-1))
                        progress.StreakDays = (progress.StreakDays ?? 0) + 1;
                    else if (progress.LastActivityDate != todayDate)
                        progress.StreakDays = 1;

                    progress.LastActivityDate = todayDate;

                    await CheckAchievements(userId);
                }
            }

            await _context.SaveChangesAsync();
        }

        // ================= Вспомогательные =================
        public async Task<UserProgress> GetOrCreateProgress(int userId)
        {
            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (progress == null)
            {
                progress = new UserProgress
                {
                    UserId = userId,
                    Xp = 0,
                    Level = 1,
                    StreakDays = 0
                };
                _context.UserProgresses.Add(progress);
                await _context.SaveChangesAsync();
            }
            return progress;
        }

        public async Task RecalculateXP(int userId)
        {
            var progress = await GetOrCreateProgress(userId);

            int xpFromTasks = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId && a.IsCorrect == true)
                .SumAsync(a => (int?)a.EarnedXp) ?? 0;

            int xpFromLessons = await _context.UserLessonCompletions
                .Where(l => l.UserId == userId)
                .SumAsync(l => (int?)l.EarnedXp) ?? 0;

            int xpFromDaily = await _context.UserDailyLogins
                .Where(x => x.UserId == userId)
                .SumAsync(x => (int?)x.EarnedXp) ?? 0;

            progress.Xp = xpFromTasks + xpFromLessons + xpFromDaily;
            progress.Level = 1 + ((progress.Xp ?? 0) / 100);

            await _context.SaveChangesAsync();
        }

        // ================= ДОСТИЖЕНИЯ =================
        public async Task CheckAchievements(int userId)
        {
            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (progress == null) return;

            var achievements = await _context.Achievements.ToListAsync();

            foreach (var achievement in achievements)
            {
                bool unlocked = false;
                switch (achievement.Type?.ToLowerInvariant())
                {
                    case "xp":
                        unlocked = (progress.Xp ?? 0) >= (achievement.TargetValue ?? 0);
                        break;
                    case "lessons":
                        var lessonsCount = await _context.UserLessonCompletions
                            .CountAsync(l => l.UserId == userId);
                        unlocked = lessonsCount >= (achievement.TargetValue ?? 0);
                        break;
                    case "tasks":
                        var tasksCount = await _context.UserTaskAttempts
                            .CountAsync(t => t.UserId == userId && t.IsCorrect == true);
                        unlocked = tasksCount >= (achievement.TargetValue ?? 0);
                        break;
                    case "streak":
                        unlocked = (progress.StreakDays ?? 0) >= (achievement.TargetValue ?? 0);
                        break;
                }

                if (!unlocked) continue;

                bool already = await _context.UserAchievements
                    .AnyAsync(a => a.UserId == userId && a.AchievementId == achievement.IdAchievement);

                if (!already)
                {
                    _context.UserAchievements.Add(new UserAchievement
                    {
                        UserId = userId,
                        AchievementId = achievement.IdAchievement,
                        EarnedAt = DateTime.Now
                    });

                    if (achievement.Xpreward.HasValue)
                        progress.Xp = (progress.Xp ?? 0) + achievement.Xpreward.Value;
                }
            }

            progress.Level = 1 + ((progress.Xp ?? 0) / 100);
            await _context.SaveChangesAsync();
        }
    }
}