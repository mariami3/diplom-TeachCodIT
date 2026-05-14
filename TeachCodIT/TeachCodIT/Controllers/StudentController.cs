using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TeachCodIT.Models;
using TeachCodIT.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Linq;
using TeachCodIT.Services;
using TeachCodIT.Models.DTOs;
using System.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TeachCodIT.Controllers
{
    [Authorize(Roles = "Студент")]
    public class StudentController : BaseController
    {
        private readonly TeachCodItContext _context;

        private readonly GamificationService _gamification;

        private readonly CodeExecutionService _codeService;

        public StudentController(TeachCodItContext context, GamificationService gamification, CodeExecutionService codeService) : base(context)
        {
            _context = context;
            _gamification = gamification;
            _codeService = codeService;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }

        // ================= DASHBOARD =================
        public async Task<IActionResult> Home()
        {
            int userId = GetUserId();

            // ================= USER =================
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.IdUser == userId);

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // ================= PROGRESS =================
            await _gamification.RecalculateXP(userId);
            var progress = await _gamification.GetOrCreateProgress(userId);

            // ================= COURSES =================
            var studentCourses = await _context.StudentCourses
                .Where(uc => uc.StudentId == userId)
                .Include(uc => uc.Course)
                    .ThenInclude(c => c.Modules)
                        .ThenInclude(m => m.Lessons)
                .ToListAsync();

            var coursesVm = new List<StudentCourseViewModel>();

            foreach (var uc in studentCourses)
            {
                var lessons = uc.Course.Modules
                    .SelectMany(m => m.Lessons)
                    .Select(l => l.IdLesson)
                    .ToList();

                
                var completedByTasks = await _context.UserTaskAttempts
                    .Where(a =>
                        a.UserId == userId &&
                        a.IsCorrect == true &&
                        a.LessonTask.Lesson.Module.CourseId == uc.Course.IdCourse
                    )
                    .Select(a => a.LessonTask.LessonId.Value)
                    .Distinct()
                    .ToListAsync();

            
                var completedManually = await _context.UserLessonCompletions
                    .Where(l => l.UserId == userId &&
                                l.Lesson.Module.CourseId == uc.Course.IdCourse)
                    .Select(l => l.LessonId)
                    .ToListAsync();

           
                var allCompleted = completedByTasks
                    .Union(completedManually)
                    .Distinct()
                    .ToList();

                int totalLessons = lessons.Count;

                int completedLessons = allCompleted.Count(l => lessons.Contains(l));

                int progressPercent = totalLessons == 0
                    ? 0
                    : (int)Math.Min(100,
                        Math.Round((double)completedLessons / totalLessons * 100));

                coursesVm.Add(new StudentCourseViewModel
                {
                    CourseId = uc.Course.IdCourse,
                    CourseTitle = uc.Course.Title,
                    ProgressPercent = progressPercent
                });
            }

            // ================= ACHIEVEMENTS =================
            var achievements = await _context.UserAchievements
                .Where(a => a.UserId == userId)
                .Include(a => a.Achievement)
                .OrderByDescending(a => a.EarnedAt)
                .Take(4)
                .Select(a => new UserAchievementViewModel
                {
                    AchievementTitle = a.Achievement.Title,
                    XPReward = a.Achievement.Xpreward ?? 0,
                    Icon = a.Achievement.Icon
                })
                .ToListAsync();

            if (!achievements.Any())
            {
                achievements = await _context.Achievements
                    .Take(4)
                    .Select(a => new UserAchievementViewModel
                    {
                        AchievementTitle = a.Title,
                        XPReward = a.Xpreward ?? 0,
                        Icon = a.Icon
                    })
                    .ToListAsync();
            }

            // ================= DAILY QUESTS =================
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var completedToday = await _context.UserDailyQuests
                .Where(x => x.UserId == userId
                         && x.QuestDate == today
                         && x.IsCompleted == true)
                .Select(x => x.QuestId)
                .ToListAsync();

            var dailyQuests = await _context.DailyQuests
                .Select(q => new UserDailyQuestViewModel
                {
                    QuestId = q.IdQuest,
                    QuestTitle = q.Title,
                    XPReward = q.Xpreward ?? 0,
                    IsCompleted = completedToday.Contains(q.IdQuest)
                })
                .ToListAsync();

            var completedDailyQuestCards = await _context.UserDailyQuests
                .Include(x => x.Quest)
                .Where(x => x.UserId == userId && x.IsCompleted == true)
                .OrderByDescending(x => x.CompletedAt)
                .Take(6)
                .Select(x => new UserAchievementViewModel
                {
                    AchievementTitle = x.Quest.Title,
                    AchievementDescription = x.Quest.Description,
                    XPReward = x.Quest.Xpreward ?? 0,
                    Icon = "fa-check-circle"
                })
                .ToListAsync();
            // ================= LEADERBOARD =================
            var leaderboardData = await _context.UserProgresses
    .Include(u => u.User)
        .ThenInclude(u => u.UserProfiles)
    .OrderByDescending(u => u.Xp)
    .Take(10)
    .ToListAsync();

            var leaderboard = leaderboardData
                .Select((u, index) => new LeaderboardEntry
                {
                    Rank = index + 1,
                    UserName = u.User.LoginUser,
                    FullName = u.User.UserProfiles
                        .Select(p => (p.FirstName ?? "") + " " + (p.LastName ?? ""))
                        .FirstOrDefault()
                        ?.Trim()
                        ?? u.User.LoginUser,
                    XP = u.Xp ?? 0,
                    StreakDays = u.StreakDays ?? 0
                })
                .ToList();

            // ================= VIEWMODEL =================
            var vm = new StudentDashboardViewModel
            {
                User = user,
                Profile = profile,
                Progress = progress,
                StudentCourses = coursesVm,
                UserAchievements = achievements,
                DailyQuests = dailyQuests,
                Leaderboard = leaderboard
            };

            return View(vm);
        }

        // ================= ПРОФИЛЬ =================
        public async Task<IActionResult> Profile()
        {
            int userId = GetUserId();

            var xpFromDaily = await _context.UserDailyLogins
                .Where(x => x.UserId == userId)
                .SumAsync(x => (int?)x.EarnedXp) ?? 0;
            // Получаем пользователя и профиль
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.IdUser == userId);

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // Получаем прогресс
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

            // Пересчёт XP (задания + уроки)
            var xpFromTasks = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId && a.IsCorrect == true)
                .SumAsync(a => (int?)a.EarnedXp) ?? 0;

            var xpFromLessons = await _context.UserLessonCompletions
                .Where(l => l.UserId == userId)
                .SumAsync(l => (int?)l.EarnedXp) ?? 0;

            progress.Xp = xpFromTasks + xpFromLessons + xpFromDaily; 
            progress.Level = 1 + (progress.Xp / 100);

            await _context.SaveChangesAsync();

            // Получаем данные достижений из базы (без обработки null)
            var recentAchievementsData = await _context.UserAchievements
                .Where(a => a.UserId == userId)
                .Include(a => a.Achievement)
                .OrderByDescending(a => a.EarnedAt)
                .Take(6)
                .ToListAsync();

            // Проекция в ViewModel с безопасной обработкой null
            var recentAchievements = recentAchievementsData
                .Select(a => new UserAchievementViewModel
                {
                    UserAchievementId = a.IdUserAchievement,
                    AchievementId = a.AchievementId ?? 0,
                    AchievementTitle = a.Achievement?.Title ?? "Неизвестно",
                    AchievementDescription = a.Achievement?.Description ?? "",
                    XPReward = a.Achievement?.Xpreward ?? 0,
                    EarnedAt = a.EarnedAt ?? DateTime.Now,
                    Icon = a.Achievement?.Icon ?? "fa-trophy",
                    IsUnlocked = true
                })
                .ToList();

            // Получаем данные активности из базы
            var recentActivityData = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AttemptDate)
                .Take(5)
                .Include(a => a.LessonTask)
                .ToListAsync();

            var recentActivity = recentActivityData
                .Select(a => new ActivityItem
                {
                    Date = a.AttemptDate ?? DateTime.Now,
                    Description = a.IsCorrect == true
                        ? $"Решено задание: {a.LessonTask?.Title ?? "Задание"}"
                        : $"Попытка задания: {a.LessonTask?.Title ?? "Задание"}",
                    EarnedXP = a.EarnedXp ?? 0,
                    Type = a.IsCorrect == true ? "success" : "fail"
                })
                .ToList();

            // Статистика
            var completedLessons = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId && a.IsCorrect == true && a.LessonTask != null)
                .Select(a => a.LessonTask.LessonId)
                .Distinct()
                .CountAsync();

            var completedTasks = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId && a.IsCorrect == true)
                .CountAsync();

            var totalCourses = await _context.StudentCourses
                .CountAsync(sc => sc.StudentId == userId);

            var completedCourses = await _context.StudentCourses
                .CountAsync(sc => sc.StudentId == userId && (sc.ProgressPercent ?? 0) >= 100);

            // Формируем ViewModel
            var vm = new ProfileViewModel
            {
                User = user,
                Profile = profile,
                Progress = progress,
                RegistrationDate = user?.RegistrationDate ?? DateTime.Now,
                CompletedCourses = completedCourses,
                TotalCourses = totalCourses,
                CompletedLessons = completedLessons,
                CompletedTasks = completedTasks,
                AchievementsCount = recentAchievements?.Count ?? 0,
                RecentAchievements = recentAchievements,
                RecentActivity = recentActivity
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string FirstName, string LastName, string Bio)
        {
            int userId = GetUserId();

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    FirstName = FirstName,
                    LastName = LastName,
                    Bio = Bio
                };
                _context.UserProfiles.Add(profile);
            }
            else
            {
                profile.FirstName = FirstName;
                profile.LastName = LastName;
                profile.Bio = Bio;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ================= КУРСЫ =================
        public async Task<IActionResult> AllCourses()
        {
            var courses = await _context.Courses
                .Where(c => c.IsPublished == true)
                .ToListAsync();
            return View(courses);
        }

        [HttpPost]
        public async Task<IActionResult> Enroll(int courseId)
        {
            int userId = GetUserId();

            var exists = await _context.StudentCourses
                .AnyAsync(sc => sc.StudentId == userId && sc.CourseId == courseId);

            if (!exists)
            {
                _context.StudentCourses.Add(new StudentCourse
                {
                    StudentId = userId,
                    CourseId = courseId,
                    ProgressPercent = 0,
                    EnrolledAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyCourses));
        }

        public async Task<IActionResult> MyCourses()
        {
            int userId = GetUserId();

            var studentCourses = await _context.StudentCourses
                .Where(sc => sc.StudentId == userId)
                .Include(sc => sc.Course)
                    .ThenInclude(c => c.Modules)
                        .ThenInclude(m => m.Lessons)
                .ToListAsync();

            var model = studentCourses.Select(sc =>
            {
                var totalLessons = sc.Course.Modules
                    .SelectMany(m => m.Lessons)
                    .Count();

                var completedLessons = _context.UserTaskAttempts
                    .Where(x =>
                        x.UserId == userId &&
                        x.IsCorrect == true &&
                        x.LessonTask.Lesson.Module.CourseId == sc.CourseId
                    )
                    .Select(x => x.LessonTask.LessonId)
                    .Distinct()
                    .Count();

                var progressPercent = totalLessons == 0
                    ? 0
                    : (int)Math.Min(100,
                        Math.Round((double)completedLessons / totalLessons * 100));

                return new StudentCourseViewModel
                {
                    StudentCourseId = sc.IdStudentCourse,
                    StudentId = sc.StudentId ?? 0,
                    CourseId = sc.CourseId ?? 0,
                    CourseTitle = sc.Course.Title,
                    CourseDescription = sc.Course.Description,
                    EnrolledAt = sc.EnrolledAt ?? DateTime.Now,

                    ModulesCount = sc.Course.Modules.Count,
                    TotalLessons = totalLessons,
                    LessonsCompleted = completedLessons,

                    ProgressPercent = progressPercent,

                    GradientColor = sc.Course.GradientColor ?? "#667eea, #764ba2"
                };
            }).ToList();

            return View(model);
        }

        // ================= ОБУЧЕНИЕ =================
        public async Task<IActionResult> Course(int id)
        {
            int userId = GetUserId();

            var course = await _context.Courses
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
                        .ThenInclude(l => l.LessonTasks)
                .FirstOrDefaultAsync(c => c.IdCourse == id);

            if (course == null) return NotFound();

            // Уроки, завершённые через задания
            var completedByTasks = await _context.UserTaskAttempts
                .Where(a =>
                    a.UserId == userId &&
                    a.IsCorrect == true &&
                    a.LessonTask.Lesson.Module.CourseId == id
                )
                .Select(a => a.LessonTask.LessonId.Value)
                .Distinct()
                .ToListAsync();

            // Уроки, завершённые вручную
            var completedManuallyIds = await _context.UserLessonCompletions
                .Where(uc =>
                    uc.UserId == userId &&
                    uc.Lesson.Module.CourseId == id   // ← ВАЖНО!
                )
                .Select(uc => uc.LessonId)
                .ToListAsync();

            // Явно приводим к List<int> и объединяем
            var allCompletedLessonIds = completedByTasks
                .Union(completedManuallyIds)               
                .Distinct()
                .ToList();

            // Общее количество уроков в курсе
            int totalLessons = course.Modules
                .Sum(m => m.Lessons.Count);

            int completedLessons = allCompletedLessonIds.Count;

            int progressPercent = totalLessons == 0
                ? 0
                : Math.Min(100, (completedLessons * 100 / totalLessons));

            var vm = new CourseDetailsViewModel
            {
                IdCourse = course.IdCourse,
                Title = course.Title,
                Description = course.Description,
                TotalLessons = totalLessons,
                TotalTasks = course.Modules.Sum(m => m.Lessons.Sum(l => l.LessonTasks.Count)),
                ProgressPercent = progressPercent,

                Modules = course.Modules
                    .OrderBy(m => m.IdModule)
                    .Select((m, index) =>
                    {
                        var moduleLessons = m.Lessons
                            .OrderBy(l => l.IdLesson)
                            .ToList();

                        // Количество завершённых уроков в модуле
                        int moduleCompleted = moduleLessons
                            .Count(l => allCompletedLessonIds.Contains(l.IdLesson));   // ← Count() с лямбдой

                        return new ModuleViewModel
                        {
                            IdModule = m.IdModule,
                            Title = m.Title,
                            OrderIndex = index + 1,

                            // Процент прогресса модуля
                            ProgressPercent = moduleLessons.Count == 0
                                ? 0
                                : (moduleCompleted * 100 / moduleLessons.Count),

                            Lessons = moduleLessons.Select((l, idx) => new LessonItemViewModel
                            {
                                IdLesson = l.IdLesson,
                                Title = l.Title,
                                XPReward = l.LessonTasks.Sum(t => t.Xpreward ?? 0),
                                TasksCount = l.LessonTasks.Count,
                                IsCompleted = allCompletedLessonIds.Contains(l.IdLesson),
                                IsLocked = idx > 0 &&
                                           !allCompletedLessonIds.Contains(moduleLessons[idx - 1].IdLesson)
                            }).ToList()
                        };
                    })
                    .ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> Lesson(int id)
        {
            int userId = GetUserId();

            var lesson = await _context.Lessons
                .Include(l => l.LessonTasks)
                .Include(l => l.Module)
                .FirstOrDefaultAsync(l => l.IdLesson == id);

            if (lesson == null)
                return NotFound();

            // Получаем все завершённые уроки в модуле (задания + ручное завершение)
            var completedByTasks = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId
                         && a.IsCorrect == true
                         && a.LessonTask.LessonId != null) 
                .Select(a => a.LessonTask.LessonId.Value) 
                .Distinct()
                .ToListAsync();

            var completedManually = await _context.UserLessonCompletions
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.LessonId)
                .ToListAsync();

            var allCompletedInModule = completedByTasks
                .Union(completedManually)
                .Distinct()
                .ToList();

            // Проверка предыдущего урока — теперь учитываем и ручное завершение
            var moduleLessons = await _context.Lessons
                .Where(l => l.ModuleId == lesson.ModuleId)
                .OrderBy(l => l.IdLesson)
                .ToListAsync();

            var currentIndex = moduleLessons.FindIndex(l => l.IdLesson == id);

            if (currentIndex > 0)
            {
                var prevLessonId = moduleLessons[currentIndex - 1].IdLesson;
                if (!allCompletedInModule.Contains(prevLessonId))
                {
                    // Предыдущий урок не завершён (ни заданиями, ни вручную) → возвращаем на курс
                    return RedirectToAction("Course", new { id = lesson.Module.CourseId });
                }
            }

            // Остальной код без изменений
            var completedTasks = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId && a.IsCorrect == true)
                .Select(a => a.LessonTaskId)
                .ToListAsync();

            bool manuallyCompleted = await _context.UserLessonCompletions
                .AnyAsync(uc => uc.UserId == userId && uc.LessonId == id);

            var vm = new LessonViewModel
            {
                IdLesson = lesson.IdLesson,
                Title = lesson.Title,
                Content = lesson.Content,
                Lesson = lesson,
                Module = lesson.Module,
                Tasks = lesson.LessonTasks.Select(t => new LessonTaskViewModel
                {
                    IdLessonTask = t.IdLessonTask,
                    Title = t.Title,
                    TaskType = t.TaskType,
                    XPReward = t.Xpreward ?? 0,
                    IsCompleted = completedTasks.Contains(t.IdLessonTask)
                }).ToList(),
                IsCompleted = lesson.LessonTasks.Any()
                    ? lesson.LessonTasks.All(t => completedTasks.Contains(t.IdLessonTask))
                    : manuallyCompleted
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> CompleteLesson(int lessonId)
        {
            int userId = GetUserId();

            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.IdLesson == lessonId);

            if (lesson == null)
                return NotFound();

            // Проверяем, не завершён ли уже вручную этим пользователем
            if (await _context.UserLessonCompletions
                .AnyAsync(uc => uc.UserId == userId && uc.LessonId == lessonId))
            {
                TempData["Info"] = "Урок уже отмечен как пройденный";
                return RedirectToAction(nameof(Lesson), new { id = lessonId });
            }

            // Запрещаем ручное завершение, если в уроке есть задания
            bool hasTasks = await _context.LessonTasks
                .AnyAsync(t => t.LessonId == lessonId);

            if (hasTasks)
            {
                TempData["Warning"] = "Уроки с заданиями завершаются автоматически после решения всех задач";
                return RedirectToAction(nameof(Lesson), new { id = lessonId });
            }

            // Добавляем запись о ручном завершении
            _context.UserLessonCompletions.Add(new UserLessonCompletion
            {
                UserId = userId,
                LessonId = lessonId,
                CompletedAt = DateTime.UtcNow,
                EarnedXp = 5
            });

            await _gamification.AddXP(userId, 5);
            await _gamification.CheckAchievements(userId);
            await _gamification.CheckDailyQuests(userId);

            // Обновляем общий прогресс пользователя
            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId);   // ← обрати внимание: User_ID, а не UserId

            if (progress != null)
            {
                progress.Xp += 5;
                progress.Level = 1 + (progress.Xp / 100);

                var today = DateOnly.FromDateTime(DateTime.Now);
                if (progress.LastActivityDate == today.AddDays(-1))
                    progress.StreakDays++;
                else if (progress.LastActivityDate != today)
                    progress.StreakDays = 1;

                progress.LastActivityDate = today;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Урок отмечен как пройденный! +5 XP";
            return RedirectToAction(nameof(Lesson), new { id = lessonId });
        }


        // ================= Слабые темы =================
        public async Task<IActionResult> WeakTopics()
        {
            int userId = GetUserId();

            var grouped = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId && a.LessonTaskId != null)
                .GroupBy(a => a.LessonTaskId.Value) 
                .Select(g => new
                {
                    TaskId = g.Key,
                    TotalAttempts = g.Count(),
                    Correct = g.Count(a => a.IsCorrect == true) 
                })
                .ToListAsync();

            var weakGrouped = grouped
                .Where(x => x.TotalAttempts > 0 && (double)x.Correct / x.TotalAttempts < 0.5)
                .ToList();

            var taskIds = weakGrouped.Select(x => x.TaskId).ToList();

            var tasks = await _context.LessonTasks
                .Where(t => taskIds.Contains(t.IdLessonTask))
                .ToDictionaryAsync(t => t.IdLessonTask);

            var result = weakGrouped.Select(x =>
            {
                var task = tasks.ContainsKey(x.TaskId) ? tasks[x.TaskId] : null;

                return new WeakTopicViewModel
                {
                    TopicName = task?.Title ?? "Задание",
                    ErrorRate = (int)((1 - (double)x.Correct / x.TotalAttempts) * 100),
                    TotalAttempts = x.TotalAttempts,
                    CorrectAnswers = x.Correct,
                    PracticeUrl = task != null
                        ? Url.Action("Tasks", new { lessonId = task.LessonId })
                        : "#"
                };
            }).ToList();

            return View(result);
        }

        public async Task<IActionResult> WeakTasks(int courseId)
        {
            int userId = GetUserId();

            var attempts = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId &&
                            a.LessonTaskId != null &&
                            a.LessonTask.Lesson.Module.CourseId == courseId)
                .Select(a => new
                {
                    LessonTaskId = a.LessonTaskId.Value, 
                    IsCorrect = a.IsCorrect ?? false     
                })
                .ToListAsync();

            var grouped = attempts
                .GroupBy(a => a.LessonTaskId)
                .Select(g => new
                {
                    TaskId = g.Key,
                    Total = g.Count(),
                    Correct = g.Count(x => x.IsCorrect)
                })
                .Where(x => x.Total >= 2 && (double)x.Correct / x.Total < 0.5)
                .ToList();

            var taskIds = grouped.Select(x => x.TaskId).ToList();

            var tasks = await _context.LessonTasks
                .Where(t => taskIds.Contains(t.IdLessonTask))
                .ToListAsync();

            var weakTasks = tasks.Select(task =>
            {
                var stat = grouped.First(x => x.TaskId == task.IdLessonTask);

                return new LessonTaskViewModel
                {
                    IdLessonTask = task.IdLessonTask,
                    Title = task.Title,
                    Description = task.Description,
                    TaskType = task.TaskType,
                    XPReward = task.Xpreward ?? 10
                };
            }).ToList();

            ViewBag.CourseId = courseId;
            ViewBag.IsRetry = true;
            ViewBag.IsWeakTasks = true;

            return View("Tasks", weakTasks);
        }

        /*[HttpGet]
        public IActionResult GetErrorChartData()
        {
            var data = _context.UserTaskAttempts
                .Where(x => x.UserId == GetUserId()) 
                .GroupBy(x => x.AttemptDate.Value.Date)
                .Select(g => new {
                    date = g.Key.ToString("dd.MM"),
                    errors = g.Count(x => x.IsCorrect == false),
                    correct = g.Count(x => x.IsCorrect == true)
                })
                .OrderBy(x => x.date)
                .ToList();

            return Json(data);
        }*/


        // ================= ЗАДАНИЯ =================
        public async Task<IActionResult> Task(int taskId)
        {
            int userId = GetUserId();

            var task = await _context.LessonTasks
                .Include(t => t.TaskOptions)
                .FirstOrDefaultAsync(t => t.IdLessonTask == taskId);

            if (task == null) return NotFound();

            var attemptsCount = await _context.UserTaskAttempts
                .CountAsync(a => a.UserId == userId && a.LessonTaskId == taskId);

            var lastAttempt = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId && a.LessonTaskId == taskId)
                .OrderByDescending(a => a.AttemptDate)
                .FirstOrDefaultAsync();

            var vm = new LessonTaskViewModel
            {
                IdLessonTask = task.IdLessonTask,
                Title = task.Title,
                Description = task.Description,
                TaskType = task.TaskType,
                XPReward = task.Xpreward ?? 0,
                Deadline = task.Deadline,
                Options = task.TaskOptions.Select(o => new TaskOptionViewModel
                {
                    IdOption = o.IdOption,
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect ?? false
                }).ToList(),

                AttemptsCount = attemptsCount,
                IsCompleted = lastAttempt?.IsCorrect == true,
                LastAttempt = lastAttempt,
                LastAttemptSummary = lastAttempt != null ? new AttemptInfo
                {
                    IsCorrect = lastAttempt.IsCorrect,
                    AttemptDate = lastAttempt.AttemptDate,
                    SubmittedAnswer = lastAttempt.SubmittedAnswer
                } : null
            };

            ViewBag.LessonId = task.LessonId;
            ViewBag.IsRetry = false;

            return View("SingleTask", vm); 
        }

        public async Task<IActionResult> Tasks(int lessonId)
        {
            int userId = GetUserId();

            var tasks = await _context.LessonTasks
                .Include(t => t.TaskOptions)
                .Where(t => t.LessonId == lessonId)
                .ToListAsync();

            // Получаем все последние попытки одним запросом
            var lastAttempts = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId && a.LessonTask.LessonId == lessonId)
                .GroupBy(a => a.LessonTaskId)
                .Select(g => g.OrderByDescending(a => a.AttemptDate).FirstOrDefault())
                .ToDictionaryAsync(
                    a => a!.LessonTaskId,
                    a => a!);

            // Получаем количество попыток (отдельно, т.к. предыдущий запрос берёт только последнюю)
            var attemptCounts = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId && a.LessonTask.LessonId == lessonId)
                .GroupBy(a => a.LessonTaskId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Count());

            var model = tasks.Select(t => new LessonTaskViewModel
            {
                IdLessonTask = t.IdLessonTask,
                Title = t.Title,
                Description = t.Description,
                TaskType = t.TaskType,
                XPReward = t.Xpreward ?? 0,

                IsCompleted = lastAttempts.TryGetValue(t.IdLessonTask, out var attempt) && attempt.IsCorrect == true,
                AttemptsCount = attemptCounts.TryGetValue(t.IdLessonTask, out var count) ? count : 0,

                // Полный объект попытки (если нужен)
                LastAttempt = lastAttempts.TryGetValue(t.IdLessonTask, out var fullAttempt) ? fullAttempt : null,

                // Упрощённая информация (если используешь LastAttemptSummary)
                LastAttemptSummary = lastAttempts.TryGetValue(t.IdLessonTask, out var summaryAttempt)
                    ? new AttemptInfo
                    {
                        IsCorrect = summaryAttempt.IsCorrect,
                        AttemptDate = summaryAttempt.AttemptDate,
                        SubmittedAnswer = summaryAttempt.SubmittedAnswer, 
                    }
                    : null,

                Options = t.TaskOptions.Select(o => new TaskOptionViewModel
                {
                    IdOption = o.IdOption,          
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect ?? false
                }).ToList()
            }).ToList();

            ViewBag.LessonId = lessonId;
            ViewBag.IsRetry = false;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PreviewCode([FromBody] CodeRequest request)
        {
            try
            {
                var task = await _context.LessonTasks.FindAsync(request.TaskId);
                if (task == null)
                    return Json(new { success = false, output = "Задание не найдено" });

                string output = RunFakeInterpreter(request.Code ?? "");

                if (output == "COMPILATION_ERROR")
                    output = "Ошибка компиляции";

                return Json(new { success = true, output });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, output = "Ошибка выполнения: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitCode([FromBody] CodeRequest request)
        {
            try
            {
                int userId = GetUserId();

                var task = await _context.LessonTasks.FindAsync(request.TaskId);
                if (task == null)
                    return Json(new { success = false, message = "Задание не найдено" });

                // Проверка попыток
                var attemptsCount = await _context.UserTaskAttempts
                    .CountAsync(a => a.UserId == userId && a.LessonTaskId == request.TaskId);

                if (attemptsCount >= 1)
                {
                    return Json(new { success = false, message = "Вы уже отправляли решение" });
                }

                // Выполнение кода
                string output = RunFakeInterpreter(request.Code ?? "");

                bool isCorrect = false;

                if (output == "COMPILATION_ERROR")
                {
                    output = "Ошибка компиляции";
                }
                else
                {
                    switch (task.CheckerType?.ToLower())
                    {
                        case "exact":
                            isCorrect = output.Trim() == (task.ExpectedOutput?.Trim() ?? "");
                            break;
                        case "contains":
                            isCorrect = !string.IsNullOrEmpty(task.ExpectedOutput) &&
                                       output.Contains(task.ExpectedOutput);
                            break;
                        case "ignore_spaces":
                            isCorrect = output.Replace(" ", "").Replace("\n", "").Replace("\r", "") ==
                                       (task.ExpectedOutput?.Replace(" ", "").Replace("\n", "").Replace("\r", "") ?? "");
                            break;
                        default:
                            isCorrect = output.Contains(task.ExpectedOutput ?? "");
                            break;
                    }
                }

                var attempt = new UserTaskAttempt
                {
                    UserId = userId,
                    LessonTaskId = task.IdLessonTask,
                    SubmittedAnswer = request.Code,
                    IsCorrect = isCorrect,
                    EarnedXp = isCorrect ? (task.Xpreward ?? 10) : 0,
                    AttemptDate = DateTime.UtcNow
                };

                _context.UserTaskAttempts.Add(attempt);

                if (isCorrect && attempt.EarnedXp > 0)
                {
                    await _gamification.AddXP(userId, attempt.EarnedXp.Value);
                    await _gamification.CheckAchievements(userId);
                    await _gamification.CheckDailyQuests(userId);
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    isCorrect,
                    output,
                    message = isCorrect ? "Правильно! 🎉" : "Неверно 😢"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SubmitCode Error: {ex.Message}");
                return Json(new { success = false, message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitTask(int taskId, int? optionId, bool isRetry = false)
        {
            int userId = GetUserId();

            var task = await _context.LessonTasks
                .Include(t => t.TaskOptions)
                .FirstOrDefaultAsync(t => t.IdLessonTask == taskId);

            if (task == null)
                return NotFound("Задание не найдено");

            // Проверяем количество попыток
            var existingAttemptsCount = await _context.UserTaskAttempts
                .CountAsync(a => a.UserId == userId && a.LessonTaskId == taskId);

            if (!isRetry && existingAttemptsCount >= 1)
            {
                TempData["Result"] = "❌ Вы исчерпали все попытки для этого задания";
                TempData["Error"] = "Повторить можно на странице 'Слабые темы' или в курсе";
                return RedirectToAction(nameof(Task), new { lessonId = task.LessonId });
            }

            bool isCorrect = false;

            if (task.TaskType == "choice" || task.TaskType == "multiple" || task.TaskType == "test")
            {
                var correctOption = task.TaskOptions.FirstOrDefault(o => o.IsCorrect == true);

                if (correctOption != null && optionId.HasValue)
                {
                    isCorrect = optionId.Value == correctOption.IdOption;
                

                }
            }

            var attempt = new UserTaskAttempt
            {
                UserId = userId,
                LessonTaskId = taskId,
                SubmittedAnswer = optionId?.ToString(),
                IsCorrect = isCorrect,
                AttemptDate = DateTime.Now,
                EarnedXp = isCorrect ? (task.Xpreward ?? 10) : 0,
                AttemptNumber = existingAttemptsCount + 1
            };

            _context.UserTaskAttempts.Add(attempt);

            if (isCorrect && attempt.EarnedXp > 0)
            {
                await _gamification.AddXP(userId, attempt.EarnedXp.Value);
                await _gamification.CheckAchievements(userId);
                await _gamification.CheckDailyQuests(userId);  
            }

            await _context.SaveChangesAsync();

            TempData["Result"] = isCorrect ? "Верно ✓" : "Ошибка ✗";
            TempData["XP"] = attempt.EarnedXp;

            return RedirectToAction(nameof(Task), new { taskId });
        }

        // ================= ПРОГРЕСС =================
        public async Task<IActionResult> Progress()
        {
            int userId = GetUserId();

            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (progress != null)
                progress.Level = 1 + (progress.Xp / 100);

            // ================= ГРАФИКИ =================
            var today = DateTime.Today;

            var days = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i))
                .Reverse()
                .ToList();

            // XP по дням
            var xpData = days.Select(day =>
            {
                var taskXp = _context.UserTaskAttempts
                    .Where(a => a.UserId == userId && a.AttemptDate.Value.Date == day)
                    .Sum(a => (int?)a.EarnedXp) ?? 0;

                var lessonXp = _context.UserLessonCompletions
                    .Where(l => l.UserId == userId && l.CompletedAt.Date == day)
                    .Sum(l => (int?)l.EarnedXp) ?? 0;

                var loginXp = _context.UserDailyLogins
                    .Where(d => d.UserId == userId && d.LoginDate == DateOnly.FromDateTime(day))
                    .Sum(d => (int?)d.EarnedXp) ?? 0;

                return taskXp + lessonXp + loginXp;
            }).ToList();

            // Активность (кол-во попыток)
            var activityData = days.Select(day =>
            {
                return _context.UserTaskAttempts
                    .Count(a => a.UserId == userId && a.AttemptDate.Value.Date == day);
            }).ToList();

            // Передаём в View
            ViewBag.WeeklyXp = xpData;
            ViewBag.WeeklyActivity = activityData;

            return View(progress);
        } 

        // ================= ЛИДЕРБОРД =================
        public async Task<IActionResult> Leaderboard(string period = "all", string sort = "xp")
        {
            int userId = GetUserId();
            var now = DateTime.Now;

            DateTime? fromDate = period switch
            {
                "week" => now.AddDays(-7),
                "month" => now.AddMonths(-1),
                _ => null
            };

            var query = _context.UserProgresses
                .Include(p => p.User)
                    .ThenInclude(u => u.UserProfiles)
                .AsQueryable();

            // если есть фильтр по периоду — считаем XP заново
            if (fromDate != null)
            {
                // Получаем XP за период
                var xpData = await _context.UserTaskAttempts
                    .Where(a => a.AttemptDate >= fromDate)
                    .GroupBy(a => a.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        XP = g.Sum(a => a.EarnedXp ?? 0)
                    })
                    .ToListAsync();

                // Сортируем в зависимости от выбранного параметра
                var sortedData = sort switch
                {
                    "streak" => xpData.OrderByDescending(x => x.XP), 
                    "level" => xpData.OrderByDescending(x => x.XP),  
                    _ => xpData.OrderByDescending(x => x.XP)
                };

                // Формируем Leaderboard
                var leaderboard = sortedData
                    .Take(10)
                    .Select((x, index) =>
                    {
                        var user = _context.Users
                            .Include(u => u.UserProfiles)
                            .FirstOrDefault(u => u.IdUser == x.UserId);

                        return new LeaderboardEntry
                        {
                            Rank = index + 1,
                            UserName = user.LoginUser,
                            FullName = user.UserProfiles
                                .Select(p => (p.FirstName ?? "") + " " + (p.LastName ?? ""))
                                .Select(name => name.Trim())
                                .FirstOrDefault(name => !string.IsNullOrEmpty(name))
                                ?? user.LoginUser,
                            XP = x.XP,
                            StreakDays = 0, 
                            IsCurrentUser = x.UserId == userId
                        };
                    })
                    .ToList();

                ViewBag.Period = period;
                ViewBag.SelectedSort = sort; 
                ViewBag.CurrentUserRank = leaderboard.FirstOrDefault(e => e.IsCurrentUser)?.Rank;

                return View(leaderboard);
            }

            var dataQuery = query;

            dataQuery = sort switch
            {
                "streak" => dataQuery.OrderByDescending(p => p.StreakDays),
                "level" => dataQuery.OrderByDescending(p => p.Level),
                _ => dataQuery.OrderByDescending(p => p.Xp)
            };

            var data = await dataQuery.ToListAsync();

            var leaderboardAll = data
                .Select((p, index) => new LeaderboardEntry
                {
                    Rank = index + 1,
                    UserName = p.User.LoginUser,
                    FullName = p.User.UserProfiles
                        .Select(up => (up.FirstName ?? "") + " " + (up.LastName ?? ""))
                        .Select(name => name.Trim())
                        .FirstOrDefault(name => !string.IsNullOrEmpty(name))
                        ?? p.User.LoginUser,
                    XP = p.Xp ?? 0,
                    StreakDays = p.StreakDays ?? 0,
                    IsCurrentUser = p.UserId == userId
                })
                .Take(10)
                .ToList();

            // позиция текущего пользователя
            var currentUserRank = data
                .Select((p, index) => new { p.UserId, Rank = index + 1 })
                .FirstOrDefault(x => x.UserId == userId);

            ViewBag.CurrentUserRank = currentUserRank?.Rank;
            ViewBag.SelectedSort = sort;
            ViewBag.SelectedPeriod = period;

            return View(leaderboardAll);
        }

        // ================= ИСТОРИЯ =================
        public async Task<IActionResult> History(int? courseId, string status)
        {
            int userId = GetUserId();

            var query = _context.UserTaskAttempts
                .Where(a => a.UserId == userId)
                .Include(a => a.LessonTask)
                    .ThenInclude(t => t.TaskOptions)
                .Include(a => a.LessonTask.Lesson)
                    .ThenInclude(l => l.Module)
                        .ThenInclude(m => m.Course)
                .AsQueryable();

            //  Фильтр по курсу
            if (courseId.HasValue)
            {
                query = query.Where(a => a.LessonTask.Lesson.Module.CourseId == courseId.Value);
            }

            //  Фильтр по статусу
            if (!string.IsNullOrEmpty(status))
            {
                switch (status)
                {
                    case "correct":
                        query = query.Where(a => a.IsCorrect == true);
                        break;
                    case "incorrect":
                        query = query.Where(a => a.IsCorrect == false);
                        break;
                    case "all":
                    default:
                        break;
                }
            }

            var history = await query
                .OrderByDescending(a => a.AttemptDate)
                .ToListAsync();

            //  список курсов для dropdown
            ViewBag.Courses = await _context.Courses.ToListAsync();
            ViewBag.SelectedStatus = status;  

            return View(history);
        }

        // ================= DAILY QUESTS =================
        public async Task<IActionResult> DailyQuests()
        {
            int userId = GetUserId();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var todayDateTime = DateTime.UtcNow.Date;

            // Прогресс
            var progress = await _context.UserProgresses.FirstOrDefaultAsync(p => p.UserId == userId);
            if (progress == null)
            {
                progress = new UserProgress { UserId = userId, Xp = 0, Level = 1, StreakDays = 0 };
                _context.UserProgresses.Add(progress);
                await _context.SaveChangesAsync();
            }

            var baseQuests = await _context.DailyQuests.ToListAsync();

            var userQuests = await _context.UserDailyQuests
                .Include(uq => uq.Quest)
                .Where(uq => uq.UserId == userId && uq.QuestDate == today)
                .ToListAsync();

            // Создаём недостающие квесты
            var existingIds = userQuests.Select(u => u.QuestId).ToHashSet();
            var missing = baseQuests
                .Where(b => !existingIds.Contains(b.IdQuest))
                .Select(b => new UserDailyQuest
                {
                    UserId = userId,
                    QuestId = b.IdQuest,
                    QuestDate = today,
                    IsCompleted = false,
                    CurrentValue = 0
                })
                .ToList();

            if (missing.Any())
            {
                _context.UserDailyQuests.AddRange(missing);
                await _context.SaveChangesAsync();

                userQuests = await _context.UserDailyQuests
                    .Include(uq => uq.Quest)
                    .Where(uq => uq.UserId == userId && uq.QuestDate == today)
                    .ToListAsync();
            }

            // ================= АВТОПРОВЕРКА =================
            foreach (var uq in userQuests.Where(q => q.IsCompleted != true))
            {
                int current = 0;
                string type = uq.Quest?.Type?.ToLowerInvariant() ?? "";

                if (type == "correct_tasks")
                {
                    current = await _context.UserTaskAttempts
                        .CountAsync(a => a.UserId == userId &&
                                         a.IsCorrect == true &&
                                         a.AttemptDate.HasValue &&
                                         a.AttemptDate.Value.Date == todayDateTime);
                }
                else if (type == "lessons")
                {
                    current = await _context.UserLessonCompletions
                        .CountAsync(l => l.UserId == userId && l.CompletedAt.Date == todayDateTime);
                }
                else if (type == "xp")
                {
                    var xpFromTasks = await _context.UserTaskAttempts
                        .Where(a => a.UserId == userId &&
                                    a.AttemptDate.HasValue &&
                                    a.AttemptDate.Value.Date == todayDateTime)
                        .SumAsync(a => (int?)a.EarnedXp) ?? 0;

                    var xpFromLessons = await _context.UserLessonCompletions
                        .Where(l => l.UserId == userId &&
                                    l.CompletedAt.Date == todayDateTime)
                        .SumAsync(l => (int?)l.EarnedXp) ?? 0;

                    var xpFromDailyLogin = await _context.UserDailyLogins
                        .Where(d => d.UserId == userId && d.LoginDate == today)
                        .SumAsync(d => (int?)d.EarnedXp) ?? 0;

                    current = xpFromTasks + xpFromLessons + xpFromDailyLogin;
                }
                else if (type == "modules")
                {
                    current = await _context.UserLessonCompletions
                        .Where(l => l.UserId == userId &&
                                    l.CompletedAt.Date == todayDateTime &&
                                    l.Lesson != null)
                        .Select(l => l.Lesson.ModuleId)
                        .Distinct()
                        .CountAsync();
                }

                uq.CurrentValue = current;

                if (current >= (uq.Quest?.TargetValue ?? 1))
                {
                    uq.IsCompleted = true;
                    uq.CompletedAt = DateTime.UtcNow;

                    int reward = uq.Quest?.Xpreward ?? 20;
                    await _gamification.AddXP(userId, reward);
                    await _gamification.CheckAchievements(userId);
                }
            }

            await _context.SaveChangesAsync();

            // Формирование 4 карточек
            var orderedTypes = new[] { "correct_tasks", "lessons", "xp", "modules" };

            var model = orderedTypes
                .Select(type => userQuests.FirstOrDefault(uq =>
                    string.Equals(uq.Quest?.Type, type, StringComparison.OrdinalIgnoreCase)))
                .Where(uq => uq != null)
                .Select(uq => new UserDailyQuestViewModel
                {
                    UserDailyQuestId = uq.IdUserDailyQuest,
                    QuestId = uq.QuestId ?? 0,
                    QuestTitle = uq.Quest?.Title ?? "Задание",
                    QuestDescription = uq.Quest?.Description ?? "",
                    XPReward = uq.Quest?.Xpreward ?? 20,
                    IsCompleted = uq.IsCompleted ?? false,
                    CurrentValue = uq.CurrentValue ?? 0,
                    TargetValue = uq.Quest?.TargetValue ?? 1,
                    QuestDate = uq.QuestDate.ToDateTime(TimeOnly.MinValue)
                })
                .ToList();

            return View(model);
        }


        // ================= ВЫПОЛНЕНИЕ ЕЖЕДНЕВНОГО ЗАДАНИЯ =================
        /*[HttpPost]
        public async Task<IActionResult> CompleteDailyQuest([FromBody] CompleteQuestRequest request)
        {
            int userDailyQuestId = request?.UserDailyQuestId ?? 0;
            int userId = GetUserId();

            Console.WriteLine($"[CompleteDailyQuest] Получен ID: {userDailyQuestId}, UserId: {userId}");

            if (userDailyQuestId <= 0)
                return Json(new { success = false, message = $"Неверный ID задания: {userDailyQuestId}" });

            var userQuest = await _context.UserDailyQuests
                .Include(uq => uq.Quest)
                .FirstOrDefaultAsync(uq => uq.IdUserDailyQuest == userDailyQuestId
                                        && uq.UserId == userId);

            if (userQuest == null)
            {
                var count = await _context.UserDailyQuests.CountAsync(u => u.UserId == userId);
                Console.WriteLine($"Найдено записей для пользователя {userId}: {count}");
                return Json(new { success = false, message = $"Задание {userDailyQuestId} не найдено для пользователя {userId}" });
            }

            if (userQuest.IsCompleted == true)
                return Json(new { success = false, message = "Задание уже выполнено" });

            userQuest.IsCompleted = true;

            int earnedXP = userQuest.Quest?.Xpreward ?? 20;

            var progress = await _context.UserProgresses.FirstOrDefaultAsync(p => p.UserId == userId);
            if (progress == null)
            {
                progress = new UserProgress { UserId = userId, Xp = 0, Level = 1, StreakDays = 0 };
                _context.UserProgresses.Add(progress);
            }

            progress.Xp += earnedXP;
            progress.Level = 1 + (progress.Xp / 100);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Задание выполнено! +{earnedXP} XP",
                earnedXP
            });
        }*/

        // ================= ПРОГРЕСС ПО КУРСУ =================
        public async Task<IActionResult> CourseProgress(int courseId)
        {
            int userId = GetUserId();

            var totalLessons = await _context.Lessons
                .Where(l => l.Module.CourseId == courseId)
                .CountAsync();

            var completedLessons = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId && a.IsCorrect == true)
                .Select(a => a.LessonTask.LessonId)
                .Distinct()
                .CountAsync();

            int percent = totalLessons == 0 ? 0 : (completedLessons * 100 / totalLessons);

            return Json(percent);
        }

        // ===================== Достижения =====================
        public async Task<IActionResult> Achievements()
        {
            int userId = GetUserId();

            // получаем или создаём прогресс
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

            
            await _gamification.RecalculateXP(userId);
            progress = await _gamification.GetOrCreateProgress(userId);

            // ================= ДОСТИЖЕНИЯ =================
            var achievements = await _context.UserAchievements
                .Where(a => a.UserId == userId)
                .Include(a => a.Achievement)
                .OrderByDescending(a => a.EarnedAt)
                .Select(a => new UserAchievementViewModel
                {
                    AchievementTitle = a.Achievement.Title,
                    AchievementDescription = a.Achievement.Description,
                    XPReward = a.Achievement.Xpreward ?? 0,
                    Icon = a.Achievement.Icon,
                    EarnedAt = a.EarnedAt ?? DateTime.Now,
                    IsUnlocked = true
                })
                .ToListAsync();

            var allAchievements = await _context.Achievements.ToListAsync();

            foreach (var a in allAchievements)
            {
                if (!achievements.Any(x => x.AchievementTitle == a.Title))
                {
                    achievements.Add(new UserAchievementViewModel
                    {
                        AchievementTitle = a.Title,
                        AchievementDescription = a.Description,
                        XPReward = a.Xpreward ?? 0,
                        Icon = a.Icon,
                        IsUnlocked = false
                    });
                }
            }

            
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var completedDailyQuests = await _context.UserDailyQuests
                .Include(q => q.Quest)
                .Where(q => q.UserId == userId
                         && q.QuestDate == today
                         && q.IsCompleted == true)
                .Select(q => new UserDailyQuestViewModel
                {
                    QuestTitle = q.Quest.Title,
                    QuestDescription = q.Quest.Description,
                    XPReward = q.Quest.Xpreward ?? 0,
                    CompletedAt = q.CompletedAt ?? DateTime.UtcNow
                })
                .ToListAsync();

            ViewBag.CompletedDailyQuests = completedDailyQuests;

            // ================= VIEWBAG =================
            ViewBag.XP = progress.Xp;
            ViewBag.Level = progress.Level;
            ViewBag.Streak = progress.StreakDays;
            ViewBag.NextLevelXP = (progress.Level + 1) * 100;
            ViewBag.Progress = progress;

            return View(achievements);
        }

        // ================= DAILY LOGIN =================
        public async Task<IActionResult> DailyLogin()
        {
            int userId = GetUserId();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.IdUser == userId);

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

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

            var today = DateOnly.FromDateTime(DateTime.Now);

            var vm = new DailyLoginViewModel
            {
                User = user,
                Profile = profile,
                Progress = progress,
                StreakDays = progress.StreakDays ?? 0
            };

            //  Уже заходил сегодня
            if (progress.LastActivityDate == today)
            {
                vm.AlreadyLogged = true;
                return View(vm);
            }

            //  безопасные значения
            int currentStreak = progress.StreakDays ?? 0;
            int currentXp = progress.Xp ?? 0;

            //  streak логика
            if (progress.LastActivityDate == today.AddDays(-1))
                currentStreak++;
            else
                currentStreak = 1;

            progress.StreakDays = currentStreak;
            progress.LastActivityDate = today;

            //  награда
            int earnedXP = 10 + (currentStreak * 2);

            currentXp += earnedXP;

            progress.Xp = currentXp;
            progress.Level = 1 + (currentXp / 100);

            _context.UserDailyLogins.Add(new UserDailyLogin
            {
                UserId = userId,
                LoginDate = today,
                EarnedXp = earnedXP
            });

            await _context.SaveChangesAsync();

            vm.Success = true;
            vm.StreakDays = currentStreak;
            vm.EarnedXP = earnedXP;
            vm.Progress = progress;

            return RedirectToAction("Home");
        }

        // ================= НАСТРОЙКИ =================
        public async Task<IActionResult> Settings()
        {
            int userId = GetUserId();

            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);
            
            

            if (settings == null)
            {
                settings = new UserSetting
                {
                    UserId = userId,
                    Theme = "light",
                    Language = "ru",
                    TimeZone = "UTC"
                };

                _context.UserSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            ViewBag.Theme = settings?.Theme ?? "light";

            var user = await _context.Users.FirstOrDefaultAsync(u => u.IdUser == userId);
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var progress = await _context.UserProgresses.FirstOrDefaultAsync(p => p.UserId == userId);
            var vm = new SettingsViewModel
            {
                Theme = settings.Theme,
                Language = settings.Language,
                TimeZone = settings.TimeZone,

                User = user,
                Profile = profile,
                Progress = progress
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Settings(UserSetting model)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                settings = new UserSetting
                {
                    UserId = userId
                };
                _context.UserSettings.Add(settings);
            }

            settings.Theme = model.Theme;
            settings.Language = model.Language;
            settings.TimeZone = model.TimeZone;

            await _context.SaveChangesAsync();

            return RedirectToAction("Settings");
        }

        private string RunFakeInterpreter(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "Пустой код";

            var variables = new Dictionary<string, int>();

            try
            {
                code = code.Replace("\r", "").Replace("\n", "").Trim();

                // === Переменные ===
                var varMatches = Regex.Matches(code, @"int\s+(\w+)\s*=\s*(\d+);");

                foreach (Match match in varMatches)
                {
                    variables[match.Groups[1].Value] = int.Parse(match.Groups[2].Value);
                }

                // === IF ===
                var ifMatch = Regex.Match(code, @"if\s*\((.*?)\)\s*\{(.*?)\}");

                if (ifMatch.Success)
                {
                    string condition = ifMatch.Groups[1].Value;
                    string body = ifMatch.Groups[2].Value;

                    foreach (var v in variables)
                        condition = Regex.Replace(condition, $@"\b{v.Key}\b", v.Value.ToString());

                    bool conditionResult = Convert.ToBoolean(new DataTable().Compute(condition, ""));

                    if (conditionResult)
                        code = body;
                    else
                        return "";
                }

                // === FOR ===
                var forMatch = Regex.Match(code,
                @"for\s*\(\s*int\s+(\w+)\s*=\s*(\d+);\s*\1\s*<\s*(\d+);\s*\1\+\+\s*\)\s*\{(.*?)\}");

                if (forMatch.Success)
                {
                    string varName = forMatch.Groups[1].Value;
                    int start = int.Parse(forMatch.Groups[2].Value);
                    int end = int.Parse(forMatch.Groups[3].Value);
                    string body = forMatch.Groups[4].Value;

                    string output = "";

                    for (int i = start; i < end; i++)
                    {
                        string tempBody = body.Replace(varName, i.ToString());

                        var match = Regex.Match(tempBody, @"Console\.WriteLine\((.*?)\);");

                        if (match.Success)
                        {
                            var result = new DataTable().Compute(match.Groups[1].Value, null);
                            output += result + "\n";
                        }
                    }

                    return output.Trim();
                }

                // === Console.WriteLine ===
                var writeMatch = Regex.Match(code, @"Console\.WriteLine\((.*?)\);");

                if (!writeMatch.Success)
                    return "Нет вывода";

                string expression = writeMatch.Groups[1].Value.Trim();

                // строка
                if (expression.StartsWith("\"") && expression.EndsWith("\""))
                    return expression.Trim('"');

                // подстановка переменных
                foreach (var v in variables)
                {
                    expression = Regex.Replace(expression, $@"\b{v.Key}\b", v.Value.ToString());
                }

                var resultFinal = new DataTable().Compute(expression, null);

                return resultFinal.ToString();
            }
            catch
            {
                return "Ошибка выполнения";
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            int userId = GetUserId();

            // Профиль
            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile != null)
                _context.UserProfiles.Remove(profile);

            // Прогресс
            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (progress != null)
                _context.UserProgresses.Remove(progress);

            // Настройки
            var settings = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings != null)
                _context.UserSettings.Remove(settings);

            // Достижения
            var achievements = await _context.UserAchievements
                .Where(a => a.UserId == userId)
                .ToListAsync();

            if (achievements.Any())
                _context.UserAchievements.RemoveRange(achievements);

            // Ежедневные задания
            var dailyQuests = await _context.UserDailyQuests
                .Where(q => q.UserId == userId)
                .ToListAsync();

            if (dailyQuests.Any())
                _context.UserDailyQuests.RemoveRange(dailyQuests);

            // Логины
            var dailyLogins = await _context.UserDailyLogins
                .Where(l => l.UserId == userId)
                .ToListAsync();

            if (dailyLogins.Any())
                _context.UserDailyLogins.RemoveRange(dailyLogins);

            // Завершенные уроки
            var lessonCompletions = await _context.UserLessonCompletions
                .Where(l => l.UserId == userId)
                .ToListAsync();

            if (lessonCompletions.Any())
                _context.UserLessonCompletions.RemoveRange(lessonCompletions);

            // Попытки решений
            var attempts = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId)
                .ToListAsync();

            if (attempts.Any())
                _context.UserTaskAttempts.RemoveRange(attempts);

            // Курсы студента
            var studentCourses = await _context.StudentCourses
                .Where(sc => sc.StudentId == userId)
                .ToListAsync();

            if (studentCourses.Any())
                _context.StudentCourses.RemoveRange(studentCourses);

            // Пользователь
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.IdUser == userId);

            if (user != null)
                _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            // Выход из аккаунта
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            // Переход на логин
            return RedirectToAction("Authorization", "Account");
        }
    }
}