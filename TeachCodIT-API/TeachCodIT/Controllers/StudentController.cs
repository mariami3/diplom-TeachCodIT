using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TeachCodIT.Models;
using TeachCodIT.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace TeachCodIT.Controllers
{
    [Authorize(Roles = "Студент")]
    public class StudentController : Controller
    {
        private readonly TeachCodItContext _context;

        public StudentController(TeachCodItContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }

        // ================= DASHBOARD =================
        public async Task<IActionResult> Home()
        {
            int userId = GetUserId();

            var user = await _context.Users.FindAsync(userId);

            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var studentCourses = await _context.StudentCourses
                .Where(sc => sc.StudentId == userId)
                .Include(sc => sc.Course)
                .ToListAsync();

            var vm = new StudentDashboardViewModel
            {
                User = user,
                Progress = progress,
                StudentCourses = studentCourses.Select(sc => new StudentCourseViewModel
                {
                    CourseId = sc.CourseId ?? 0,
                    CourseTitle = sc.Course.Title,
                    ProgressPercent = sc.ProgressPercent ?? 0
                }).ToList(),
                TotalCourses = studentCourses.Count
            };

            return View(vm);
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

            var model = studentCourses.Select(sc => new StudentCourseViewModel
            {
                StudentCourseId = sc.IdStudentCourse,
                StudentId = sc.StudentId ?? 0,
                CourseId = sc.CourseId ?? 0,
                CourseTitle = sc.Course.Title,
                CourseDescription = sc.Course.Description,
                EnrolledAt = sc.EnrolledAt ?? DateTime.Now,
                ProgressPercent = sc.ProgressPercent ?? 0,
                OverallProgressPercent = sc.ProgressPercent ?? 0,
                GradientColor = sc.Course.GradientColor ?? "#667eea, #764ba2",
                ModulesCount = sc.Course.Modules.Count,
                TotalLessons = sc.Course.Modules.Sum(m => m.Lessons.Count),
                LessonsCompleted = (sc.ProgressPercent ?? 0) * sc.Course.Modules.Sum(m => m.Lessons.Count) / 100
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
                .FirstOrDefaultAsync(c => c.IdCourse == id);

            if (course == null)
                return NotFound();

            
            var completedLessonIds = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId && a.IsCorrect == true)
                .Select(a => a.LessonTask.LessonId)
                .Distinct()
                .ToListAsync();

            var courseDetailsVm = new CourseDetailsViewModel
            {
                IdCourse = course.IdCourse,
                Title = course.Title,
                Description = course.Description,
                Modules = course.Modules.Select(m => new ModuleViewModel
                {
                    IdModule = m.IdModule,
                    Title = m.Title,
                    Lessons = m.Lessons
                    .OrderBy(l => l.IdLesson) 
                    .Select((l, index) => new LessonItemViewModel
                    {
                      IdLesson = l.IdLesson,
                      Title = l.Title,

                      IsCompleted = completedLessonIds.Contains(l.IdLesson),

                      IsLocked = index > 0 &&
                      !completedLessonIds.Contains(m.Lessons
                      .OrderBy(x => x.IdLesson)
                      .ElementAt(index - 1).IdLesson)
                    }).ToList()
                }).ToList()
            };

            return View(courseDetailsVm);
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

            var moduleLessons = await _context.Lessons
                .Where(l => l.ModuleId == lesson.ModuleId)
                .OrderBy(l => l.IdLesson)
                .ToListAsync();

            var currentIndex = moduleLessons.FindIndex(l => l.IdLesson == id);

            if (currentIndex > 0)
            {
                var prevLessonId = moduleLessons[currentIndex - 1].IdLesson;

                var completedLessons = await _context.UserTaskAttempts
                    .Where(a => a.UserId == userId && a.IsCorrect == true)
                    .Select(a => a.LessonTask.LessonId)
                    .Distinct()
                    .ToListAsync();

                if (!completedLessons.Contains(prevLessonId))
                {
                    return RedirectToAction("Course", new { id = lesson.Module.CourseId });
                }
            }

            var module = await _context.Modules
                .FirstOrDefaultAsync(m => m.IdModule == lesson.ModuleId);

            var completedTasks = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId && a.IsCorrect == true)
                .Select(a => a.LessonTaskId)
                .ToListAsync();

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

                IsCompleted = completedTasks.Any()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> CompleteLesson(int lessonId)
        {
            int userId = GetUserId();

            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (progress != null)
            {
                progress.Xp += 10;
                progress.Level = 1 + (progress.Xp / 100);

                var today = DateOnly.FromDateTime(DateTime.Now);
                if (progress.LastActivityDate == today.AddDays(-1))
                    progress.StreakDays++;
                else if (progress.LastActivityDate != today)
                    progress.StreakDays = 1;

                progress.LastActivityDate = today;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Lesson), new { id = lessonId });
        }


        // ================= Слабые темы =================
        public async Task<IActionResult> WeakTopics()
        {
            int userId = GetUserId();

            // Пример: задания, где > 50% попыток неверные
            var weakTasks = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId && a.IsCorrect == false)
                .GroupBy(a => a.LessonTaskId)
                .Select(g => new
                {
                    TaskId = g.Key,
                    TotalAttempts = g.Count(),
                    Correct = g.Count(a => a.IsCorrect == true),
                    LessonTask = g.First().LessonTask
                })
                .Where(x => x.TotalAttempts > 0 && (double)x.Correct / x.TotalAttempts < 0.5)
                .Select(x => new WeakTopicViewModel
                {
                    TopicName = x.LessonTask.Title,
                    ErrorRate = (int)((1 - (double)x.Correct / x.TotalAttempts) * 100),
                    TotalAttempts = x.TotalAttempts,
                    CorrectAnswers = x.Correct,
                    PracticeUrl = Url.Action("Tasks", new { lessonId = x.LessonTask.LessonId })
                })
                .ToListAsync();

            return View(weakTasks);
        }

        public async Task<IActionResult> WeakTasks(int courseId)
        {
            int userId = GetUserId();

            var weakTasks = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId
                         && a.IsCorrect == false
                         && a.LessonTask.Lesson.Module.CourseId == courseId)
                .GroupBy(a => a.LessonTaskId)
                .Select(g => new
                {
                    Task = g.First().LessonTask,
                    Total = g.Count(),
                    Correct = g.Count(a => a.IsCorrect == true)
                })
                .Where(x => x.Total >= 2 && (double)x.Correct / x.Total < 0.5)
                .Select(x => new LessonTaskViewModel
                {
                    IdLessonTask = x.Task.IdLessonTask,
                    Title = x.Task.Title,
                    Description = x.Task.Description,
                    TaskType = x.Task.TaskType,
                    XPReward = x.Task.Xpreward ?? 10,
                    // Можно добавить AttemptsCount и LastAttemptSummary, если нужно
                })
                .ToListAsync();

            ViewBag.CourseId = courseId;
            ViewBag.IsRetry = true;           // флаг, что это повтор
            ViewBag.IsWeakTasks = true;       // опционально — для заголовка "Слабые задания"

            return View("Tasks", weakTasks);  // используем то же представление, что и для обычного списка
        }
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
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect ?? false
                }).ToList()
            }).ToList();

            ViewBag.LessonId = lessonId;
            ViewBag.IsRetry = false;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitTask(int taskId, string? answer, bool isRetry = false)
        {
            int userId = GetUserId();

            var task = await _context.LessonTasks
                .Include(t => t.TaskOptions)
                .FirstOrDefaultAsync(t => t.IdLessonTask == taskId);

            if (task == null) return NotFound("Задание не найдено");

            // Проверяем количество уже сделанных попыток
            var existingAttemptsCount = await _context.UserTaskAttempts
                .CountAsync(a => a.UserId == userId && a.LessonTaskId == taskId);


            if (!isRetry && existingAttemptsCount >= 1)
            {
                TempData["Result"] = "❌ Вы исчерпали все попытки для этого задания";
                TempData["Error"] = "Повторить можно на странице 'Слабые темы' или в курсе";
                return RedirectToAction(nameof(Tasks), new { lessonId = task.LessonId });
            }

            bool isCorrect = false;

            if (task.TaskType == "text" || task.TaskType == "code")
            {
                isCorrect = !string.IsNullOrWhiteSpace(answer);
            }
            else if (task.TaskType == "choice" || task.TaskType == "multiple")
            {
                var correctOption = task.TaskOptions.FirstOrDefault(o => o.IsCorrect == true);
                if (correctOption != null && !string.IsNullOrWhiteSpace(answer))
                {
                    string cleanAnswer = answer
                        .Replace("А)", "").Replace("Б)", "").Replace("В)", "").Replace("Г)", "")
                        .Replace("A)", "").Replace("B)", "").Replace("C)", "").Replace("D)", "")
                        .Trim();

                    string cleanCorrect = correctOption.OptionText.Trim();

                    isCorrect = string.Equals(cleanAnswer, cleanCorrect, StringComparison.OrdinalIgnoreCase);
                }
            }

            var attempt = new UserTaskAttempt
            {
                UserId = userId,
                LessonTaskId = taskId,
                SubmittedAnswer = answer,
                IsCorrect = isCorrect,
                AttemptDate = DateTime.Now,
                EarnedXp = isCorrect ? (task.Xpreward ?? 10) : 0,
                AttemptNumber = existingAttemptsCount + 1
            };

            _context.UserTaskAttempts.Add(attempt);

            if (isCorrect && attempt.EarnedXp > 0)
            {
                var progress = await _context.UserProgresses
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (progress != null)
                {
                    progress.Xp += attempt.EarnedXp.Value;
                    progress.Level = 1 + (progress.Xp / 100);

                    var today = DateOnly.FromDateTime(DateTime.Now);
                    if (progress.LastActivityDate == today.AddDays(-1))
                        progress.StreakDays++;
                    else if (progress.LastActivityDate != today)
                        progress.StreakDays = 1;

                    progress.LastActivityDate = today;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Result"] = isCorrect ? "Верно" : "Ошибка";
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

            return View(progress);
        }

        // ================= ЛИДЕРБОРД =================
        public async Task<IActionResult> Leaderboard()
        {
            var leaderboard = await _context.UserProgresses
                .Include(p => p.User)
                .OrderByDescending(p => p.Xp)
                .Take(10)
                .ToListAsync();

            return View(leaderboard);
        }

        // ================= ИСТОРИЯ =================
        public async Task<IActionResult> History()
        {
            int userId = GetUserId();

            var history = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId)
                .Include(a => a.LessonTask)
                .ToListAsync();

            return View(history);
        }


        // ================= DAILY QUESTS =================
        public async Task<IActionResult> DailyQuests()
        {
            int userId = GetUserId();

            var quests = await _context.UserDailyQuests
                .Where(q => q.UserId == userId)
                .Include(q => q.Quest)
                .ToListAsync();

            return View(quests);
        }

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
    }
}