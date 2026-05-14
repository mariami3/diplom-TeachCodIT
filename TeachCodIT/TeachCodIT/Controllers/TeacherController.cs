using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net.Http.Json;
using TeachCodIT.Models;
using TeachCodIT.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TeachCodIT.Services;
using Microsoft.EntityFrameworkCore;

namespace TeachCodIT.Controllers
{
    [Authorize(Roles = "Учитель")]
    public class TeacherController : Controller
    {
        private readonly CourseApiService _courseService;
        private readonly ModuleApiService _moduleService;
        private readonly LessonApiService _lessonService;
        private readonly LessonTaskApiService _taskService;
        private readonly TaskOptionApiService _optionService;
        private readonly StudentCourseApiService _studentCourseService;
        private readonly UserTaskAttemptApiService _attemptService;
        private readonly UserApiService _userApiService;
        private readonly TeachCodItContext _context;
        private readonly EmailService _emailService;

        public TeacherController(

            TeachCodItContext context,
            CourseApiService courseService,
            ModuleApiService moduleService,
            LessonApiService lessonService,
            LessonTaskApiService taskService,
            TaskOptionApiService optionService,
            StudentCourseApiService studentCourseService,
            UserTaskAttemptApiService attemptService,
            UserApiService userApiService,
            EmailService emailService
)
        {
            _context = context;
            _courseService = courseService;
            _moduleService = moduleService;
            _lessonService = lessonService;
            _taskService = taskService;
            _optionService = optionService;
            _studentCourseService = studentCourseService;
            _attemptService = attemptService;
            _userApiService = userApiService;
            _emailService = emailService;
        }

        // ===================== Курсы =====================
        public async Task<IActionResult> Dashboard()
        {
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var courses = await _context.Courses
                .Where(c => c.CreatedBy == teacherId)
                .Select(c => new Course
                {
                    IdCourse = c.IdCourse,
                    Title = c.Title,
                    CreatedAt = c.CreatedAt,
                    IsPublished = c.IsPublished,
                    CreatedBy = c.CreatedBy,

                    StudentCourses = c.StudentCourses
                })
                .ToListAsync();

            var totalStudents = await _context.StudentCourses
                .Where(sc => sc.Course.CreatedBy == teacherId)
                .Select(sc => sc.StudentId)
                .Distinct()
                .CountAsync();

            var totalTasks = await _context.LessonTasks
                .Where(t => t.Lesson.Module.Course.CreatedBy == teacherId)
                .CountAsync();

            var topStudents = await _context.UserProgresses
                .Include(p => p.User)
                .OrderByDescending(p => p.Xp)
                .Take(3)
                .ToListAsync();

            var model = new TeacherDashboardViewModel
            {
                TeacherName = User.Identity.Name,
                TotalCourses = courses.Count,
                TotalStudents = totalStudents,
                TotalTasks = totalTasks,
                ActiveStudentsToday = 5,
                NewStudentsWeek = 3,
                PendingTasks = 7,
                AverageProgress = 45,
                Courses = courses,
                TopStudents = topStudents.Select(p => p.User).ToList()
            };

            return View(model);
        }

        public IActionResult CreateCourse() => View();

        [HttpPost]
        public async Task<IActionResult> CreateCourse(Course course)
        {
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            course.CreatedBy = teacherId;
            course.CreatedAt = DateTime.Now;

            await _courseService.CreateCourseAsync(course);

            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> EditCourse(Course course)
        {
            await _courseService.UpdateCourseAsync(course.IdCourse, course);
            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> PublishCourse(int id)
        {
            await _courseService.PublishCourseAsync(id);
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            await _courseService.DeleteCourseAsync(id);
            return RedirectToAction(nameof(Dashboard));
        }

        // ===================== Модули =====================
        public async Task<IActionResult> Modules(int courseId)
        {
            var modules = await _moduleService.GetModulesByCourseAsync(courseId);
            ViewBag.CourseId = courseId;
            return View(modules);
        }

        public IActionResult CreateModule(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateModule(Module module)
        {
            await _moduleService.CreateModuleAsync(module);

            var students = await _context.StudentCourses
                .Where(sc => sc.CourseId == module.CourseId)
                .Select(sc => sc.Student)
                .ToListAsync();

            foreach (var student in students)
            {
                string subject = "Новый модуль в курсе";

                string body = $@"Здравствуйте, {student.LoginUser}!

В курсе был добавлен новый модуль: {module.Title}.

Зайдите в платформу TeachCodIT, чтобы начать обучение.

С уважением,
Команда TeachCodIT";

                await _emailService.SendEmailAsync(
                    student.Email,
                    subject,
                    body
                );
            }

            return RedirectToAction(nameof(Modules), new { courseId = module.CourseId });
        }

        public async Task<IActionResult> EditModule(int id)
        {
            var module = await _moduleService.GetModuleByIdAsync(id);
            return View(module);
        }

        [HttpPost]
        public async Task<IActionResult> EditModule(Module module)
        {
            await _moduleService.UpdateModuleAsync(module.IdModule, module);

            var students = await _context.StudentCourses
                .Where(sc => sc.CourseId == module.CourseId)
                .Select(sc => sc.Student)
                .ToListAsync();

            foreach (var student in students)
            {
                string subject = "Изменение модуля в курсе";

                string body = $@"Здравствуйте, {student.LoginUser}!

Модуль в курсе был обновлён: {module.Title}.

Проверьте изменения в платформе TeachCodIT.

С уважением,
Команда TeachCodIT";

                await _emailService.SendEmailAsync(
                    student.Email,
                    subject,
                    body
                );
            }

            return RedirectToAction(nameof(Modules), new { courseId = module.CourseId });
        }

        public async Task<IActionResult> DeleteModule(int id, int courseId)
        {
            var module = await _moduleService.GetModuleByIdAsync(id);

            await _moduleService.DeleteModuleAsync(id);

            var students = await _context.StudentCourses
                .Where(sc => sc.CourseId == courseId)
                .Select(sc => sc.Student)
                .ToListAsync();

            foreach (var student in students)
            {
                string subject = "Удаление модуля из курса";

                string body = $@"Здравствуйте, {student.LoginUser}!

Из курса был удалён модуль: {module?.Title}.

Некоторые материалы могут быть недоступны.

С уважением,
Команда TeachCodIT";

                await _emailService.SendEmailAsync(
                    student.Email,
                    subject,
                    body
                );
            }

            return RedirectToAction(nameof(Modules), new { courseId });
        }

        // ===================== Уроки =====================
        public async Task<IActionResult> Lessons(int moduleId)
        {
            if (moduleId <= 0)
                return BadRequest("moduleId некорректный");

            var lessons = await _lessonService.GetLessonsByModuleAsync(moduleId);

            var module = await _moduleService.GetModuleByIdAsync(moduleId);

            if (module == null)
                return NotFound("Модуль не найден");

            ViewBag.ModuleId = moduleId;
            ViewBag.CourseId = module.CourseId;

            return View(lessons);
        }

        public IActionResult CreateLesson(int moduleId)
        {
            ViewBag.ModuleId = moduleId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateLesson(Lesson lesson)
        {
            await _lessonService.CreateLessonAsync(lesson);

            var module = await _context.Modules
                .FirstOrDefaultAsync(m => m.IdModule == lesson.ModuleId);

            var students = await _context.StudentCourses
                .Where(sc => sc.CourseId == module.CourseId)
                .Select(sc => sc.Student)
                .ToListAsync();

            foreach (var student in students)
            {
                string subject = "Новый урок в модуле";

                string body = $@"Здравствуйте, {student.LoginUser}!

В модуле был добавлен новый урок: {lesson.Title}.

Перейдите в платформу TeachCodIT, чтобы продолжить обучение.

С уважением,
Команда TeachCodIT";

                await _emailService.SendEmailAsync(
                    student.Email,
                    subject,
                    body
                );
            }

            return RedirectToAction(nameof(Lessons), new { moduleId = lesson.ModuleId });
        }

        public async Task<IActionResult> EditLesson(int id)
        {
            var lesson = await _lessonService.GetLessonByIdAsync(id);
            return View(lesson);
        }

        [HttpPost]
        public async Task<IActionResult> EditLesson(Lesson lesson)
        {
            await _lessonService.UpdateLessonAsync(lesson.IdLesson, lesson);

            var module = await _context.Modules
                .FirstOrDefaultAsync(m => m.IdModule == lesson.ModuleId);

            var students = await _context.StudentCourses
                .Where(sc => sc.CourseId == module.CourseId)
                .Select(sc => sc.Student)
                .ToListAsync();

            foreach (var student in students)
            {
                string subject = "Обновление урока";

                string body = $@"Здравствуйте, {student.LoginUser}!

Урок был обновлён: {lesson.Title}.

Проверьте новые материалы в TeachCodIT.

С уважением,
Команда TeachCodIT";

                await _emailService.SendEmailAsync(
                    student.Email,
                    subject,
                    body
                );
            }

            return RedirectToAction(nameof(Lessons), new { moduleId = lesson.ModuleId });
        }

        public async Task<IActionResult> DeleteLesson(int id, int moduleId)
        {
            var lesson = await _lessonService.GetLessonByIdAsync(id);

            var module = await _context.Modules
                .FirstOrDefaultAsync(m => m.IdModule == moduleId);

            await _lessonService.DeleteLessonAsync(id);

            var students = await _context.StudentCourses
                .Where(sc => sc.CourseId == module.CourseId)
                .Select(sc => sc.Student)
                .ToListAsync();

            foreach (var student in students)
            {
                string subject = "Удаление урока";

                string body = $@"Здравствуйте, {student.LoginUser}!

Из модуля был удалён урок: {lesson?.Title}.

Некоторые материалы больше недоступны.

С уважением,
Команда TeachCodIT";

                await _emailService.SendEmailAsync(
                    student.Email,
                    subject,
                    body
                );
            }

            return RedirectToAction(nameof(Lessons), new { moduleId });
        }

        // ===================== Задания и тесты =====================
        public async Task<IActionResult> Tasks(int lessonId, int moduleId)
        {
            var tasks = await _taskService.GetTasksByLessonAsync(lessonId);

            ViewBag.LessonId = lessonId;
            ViewBag.ModuleId = moduleId;

            return View(tasks);
        }

        public async Task<IActionResult> CreateTask(int lessonId)
        {
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);

            if (lesson == null)
                return NotFound();

            ViewBag.LessonId = lessonId;
            ViewBag.ModuleId = lesson.ModuleId;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(LessonTask task)
        {
            if (task.TaskType == "code")
            {
                if (string.IsNullOrWhiteSpace(task.ExpectedOutput))
                {
                    ModelState.AddModelError("", "Укажите ожидаемый вывод");
                    return View(task);
                }

                if (string.IsNullOrWhiteSpace(task.CheckerType))
                {
                    ModelState.AddModelError("", "Укажите тип проверки");
                    return View(task);
                }
            }

            await _taskService.CreateTaskAsync(task);

            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.IdLesson == task.LessonId);

            if (lesson == null)
                return NotFound();

            return RedirectToAction(nameof(Tasks), new
            {
                lessonId = lesson.IdLesson,
                moduleId = lesson.ModuleId
            });
        }

        public async Task<IActionResult> EditTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> EditTask(LessonTask task)
        {
            if (task.TaskType == "code" && string.IsNullOrWhiteSpace(task.ExpectedOutput))
            {
                ModelState.AddModelError("", "Ожидаемый вывод обязателен");
                return View(task);
            }

            await _taskService.UpdateTaskAsync(task.IdLessonTask, task);

            if (task.LessonId is not int lessonId)
                return BadRequest();

            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.IdLesson == lessonId);

            return RedirectToAction(nameof(Tasks), new
            {
                lessonId = task.LessonId,
                moduleId = lesson.ModuleId
            });
        }

        public async Task<IActionResult> DeleteTask(int id, int lessonId)
        {
            await _taskService.DeleteTaskAsync(id);
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);

            return RedirectToAction(nameof(Tasks), new
            {
                lessonId = lessonId,
                moduleId = lesson.ModuleId
            });
        }

        // ===================== Опции тестов =====================
        public async Task<IActionResult> Options(int taskId)
        {
            var options = await _optionService.GetOptionsByTaskAsync(taskId);

            var task = await _taskService.GetTaskByIdAsync(taskId);

            ViewBag.TaskId = taskId;
            ViewBag.TaskType = task.TaskType;
            ViewBag.LessonId = task.LessonId;   

            return View(options);
        }


        public async Task<IActionResult> CreateOption(TaskOption option)
        {
            await _optionService.CreateOptionAsync(option);
            return RedirectToAction(nameof(Options), new { taskId = option.LessonTaskId });
        }

        public async Task<IActionResult> EditOption(TaskOption option)
        {
            await _optionService.UpdateOptionAsync(option.IdOption, option);
            return RedirectToAction(nameof(Options), new { taskId = option.LessonTaskId });
        }

        public async Task<IActionResult> DeleteOption(int id, int taskId)
        {
            await _optionService.DeleteOptionAsync(id);
            return RedirectToAction(nameof(Options), new { taskId });
        }

        // ===================== Работа со студентами =====================
        public async Task<IActionResult> Students(int? courseId)
        {
            ViewBag.CourseId = courseId ?? 0;

            var allStudents = await _userApiService.GetStudentsAsync();
            ViewBag.AllStudents = allStudents;

            if (courseId == null)
                return View(new List<StudentCourseViewModel>());

            var students = await _studentCourseService.GetStudentsByCourseAsync(courseId.Value);

            var totalTasks = await _context.LessonTasks
                .Where(t => t.Lesson.Module.CourseId == courseId.Value)
                .CountAsync();

            foreach (var student in students)
            {
                var progress = await _context.UserProgresses
                    .FirstOrDefaultAsync(p => p.UserId == student.StudentId);

                student.TotalXP = progress?.Xp ?? 0;
                student.StreakDays = progress?.StreakDays ?? 0;

                var completedTasks = await _context.UserTaskAttempts
                    .Where(a =>
                        a.UserId == student.StudentId &&
                        a.IsCorrect == true)
                    .Select(a => a.LessonTaskId)
                    .Distinct()
                    .CountAsync();

                student.OverallProgressPercent = totalTasks == 0
                    ? 0
                    : (completedTasks * 100 / totalTasks);
            }

            return View(students);
        }

        [HttpPost]
        public async Task<IActionResult> AssignCourse(int studentId, int courseId)
        {
            bool success = await _studentCourseService.EnrollAsync(studentId, courseId);

            if (success)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.IdUser == studentId);

                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.IdCourse == courseId);

                if (user != null && course != null)
                {
                    string subject = "Вы добавлены на курс";

                    string body = $@"Здравствуйте, {user.LoginUser}!

Вы были добавлены на курс: {course.Title}.

Теперь он доступен в вашем личном кабинете TeachCodIT.

С уважением,
Команда TeachCodIT";

                    await _emailService.SendEmailAsync(
                        user.Email,
                        subject,
                        body
                    );
                }

                TempData["Success"] = "Студент успешно назначен на курс";
            }
            else
            {
                TempData["Error"] = "Не удалось назначить курс";
            }

            return RedirectToAction(nameof(Students), new { courseId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveStudentFromCourse(int studentId, int courseId)
        {
            var enrollment = await _context.StudentCourses
                .FirstOrDefaultAsync(x => x.StudentId == studentId && x.CourseId == courseId);

            if (enrollment == null)
            {
                TempData["Error"] = "Запись не найдена";
                return RedirectToAction(nameof(Students), new { courseId });
            }

            var result = await _studentCourseService.UnenrollAsync(enrollment.IdStudentCourse);

            if (result)
            {
             
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.IdUser == studentId);

                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.IdCourse == courseId);

                if (user != null && course != null)
                {
                    string subject = "Вы удалены с курса";

                    string body = $@"Здравствуйте, {user.LoginUser}!

Вы были удалены с курса: {course.Title}.

Если это ошибка — обратитесь к преподавателю.

С уважением,  
Команда TeachCodIT";

                    await _emailService.SendEmailAsync(user.Email, subject, body);
                }

                TempData["Success"] = "Студент удалён из курса";
            }
            else
            {
                TempData["Error"] = "Не удалось удалить студента";
            }

            return RedirectToAction(nameof(Students), new { courseId });
        }


        public async Task<IActionResult> Progress(int studentId, int courseId)
        {
            var enrollment = await _studentCourseService.GetEnrollmentAsync(studentId, courseId);
            return View(enrollment);
        }

        public async Task<IActionResult> UpdateProgress(int studentId, int courseId, int progressPercent)
        {
            await _studentCourseService.UpdateProgressAsync(studentId, courseId, progressPercent);
            return RedirectToAction(nameof(Progress), new { studentId, courseId });
        }

        // ===================== Аналитика =====================
        public async Task<IActionResult> Analytics(int courseId)
        {
            var students = await _context.StudentCourses
                .Include(sc => sc.Student)
                .Include(sc => sc.Student.UserProgress)
                .Where(sc => sc.CourseId == courseId)
                .ToListAsync();

            int totalStudents = students.Count;

            int completedStudents = students.Count(s => s.ProgressPercent >= 100);

            int avgProgress = students.Any()
                ? (int)students.Average(s => s.ProgressPercent ?? 0)
                : 0;

            // 📌 ВСЕ задания курса
            var tasks = await _context.LessonTasks
                .Include(t => t.Lesson)
                .ThenInclude(l => l.Module)
                .Where(t => t.Lesson.Module.CourseId == courseId)
                .ToListAsync();

            var taskAnalytics = new List<TaskAnalyticsViewModel>();

            foreach (var task in tasks)
            {
                var attempts = await _context.UserTaskAttempts
                    .Where(a => a.LessonTaskId == task.IdLessonTask)
                    .ToListAsync();

                var completedCount = attempts
                    .Where(a => a.IsCorrect == true)
                    .Select(a => a.UserId)
                    .Distinct()
                    .Count();

                var avgScore = attempts.Any()
                    ? attempts.Average(a => a.EarnedXp ?? 0)
                    : 0;

                taskAnalytics.Add(new TaskAnalyticsViewModel
                {
                    Title = task.Title,
                    Type = task.TaskType,
                    CompletedCount = completedCount,
                    TotalStudents = totalStudents,
                    AverageScore = Math.Round(avgScore, 1),
                    Deadline = task.Deadline
                });
            }

            var model = new TeacherAnalyticsViewModel
            {
                AverageProgress = avgProgress,
                TotalStudents = totalStudents,
                CompletedStudents = completedStudents,
                Tasks = taskAnalytics
            };

            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = await _context.Courses
                .Where(c => c.IdCourse == courseId)
                .Select(c => c.Title)
                .FirstOrDefaultAsync();

            return View(model);
        }

        public async Task<IActionResult> ModuleAnalytics(int moduleId)
        {
            var module = await _context.Modules
                .Include(m => m.Lessons)
                    .ThenInclude(l => l.LessonTasks)
                .FirstOrDefaultAsync(m => m.IdModule == moduleId);

            if (module == null) return NotFound();

            // Все студенты курса
            var students = await _context.StudentCourses
                .Include(sc => sc.Student)
                    .ThenInclude(s => s.UserProfiles)
                .Include(sc => sc.Student.UserProgress)
                .Where(sc => sc.CourseId == module.CourseId)
                .ToListAsync();

            var result = new ModuleAnalyticsViewModel
            {
                ModuleId = moduleId,
                ModuleTitle = module.Title,
                Students = students.Select(s => new StudentModuleResultViewModel
                {
                    StudentId = s.StudentId ?? 0,
                    FullName = $"{s.Student.UserProfiles.FirstOrDefault()?.FirstName} {s.Student.UserProfiles.FirstOrDefault()?.LastName}".Trim(),
                    TotalXP = s.Student.UserProgress?.Xp ?? 0,

                    Tasks = module.Lessons
                        .SelectMany(l => l.LessonTasks)
                        .Select(t =>
                        {
                            var attempt = _context.UserTaskAttempts
                                .Where(a => a.UserId == s.StudentId && a.LessonTaskId == t.IdLessonTask)
                                .OrderByDescending(a => a.AttemptDate)
                                .FirstOrDefault();

                            return new TaskResultViewModel
                            {
                                TaskId = t.IdLessonTask,
                                TaskTitle = t.Title,
                                TaskType = t.TaskType ?? "test",
                                IsCorrect = attempt?.IsCorrect,
                                EarnedXP = attempt?.EarnedXp,
                                LastAttempt = attempt?.AttemptDate
                            };
                        }).ToList()
                }).ToList()
            };

            return View(result);
        }
        public async Task<IActionResult> ExportModuleAnalyticsToExcel(int moduleId)
        {
            var module = await _context.Modules
                .Include(m => m.Lessons)
                    .ThenInclude(l => l.LessonTasks)
                .FirstOrDefaultAsync(m => m.IdModule == moduleId);

            if (module == null) return NotFound();

            var students = await _context.StudentCourses
                .Include(sc => sc.Student)
                    .ThenInclude(s => s.UserProfiles)
                .Include(sc => sc.Student.UserProgress)
                .Where(sc => sc.CourseId == module.CourseId)
                .ToListAsync();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Аналитика");

                // Заголовки
                worksheet.Cell(1, 1).Value = "Студент";
                worksheet.Cell(1, 2).Value = "XP";

                var tasks = module.Lessons.SelectMany(l => l.LessonTasks).ToList();

                for (int i = 0; i < tasks.Count; i++)
                {
                    worksheet.Cell(1, i + 3).Value = tasks[i].Title;
                }

                int row = 2;

                foreach (var s in students)
                {
                    var fullName = $"{s.Student.UserProfiles.FirstOrDefault()?.FirstName} {s.Student.UserProfiles.FirstOrDefault()?.LastName}".Trim();
                    worksheet.Cell(row, 1).Value = fullName;
                    worksheet.Cell(row, 2).Value = s.Student.UserProgress?.Xp ?? 0;

                    for (int i = 0; i < tasks.Count; i++)
                    {
                        var t = tasks[i];

                        var attempt = await _context.UserTaskAttempts
                            .Where(a => a.UserId == s.StudentId && a.LessonTaskId == t.IdLessonTask)
                            .OrderByDescending(a => a.AttemptDate)
                            .FirstOrDefaultAsync();

                        worksheet.Cell(row, i + 3).Value =
                            attempt?.IsCorrect == true ? "✔"
                            : attempt?.IsCorrect == false ? "✘"
                            : "-";
                    }

                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "module_analytics.xlsx");
                }
            }
        }
        // ===================== Ручная проверка заданий =====================

        public async Task<IActionResult> Solutions(int lessonId)
        {
            var attempts = await _attemptService.GetAttemptsByLessonAsync(lessonId);

            ViewBag.LessonId = lessonId;

            return View(attempts);
        }

        public async Task<IActionResult> ReviewTask(int attemptId)
        {
            var attempt = await _attemptService.GetUserTaskAttemptByIdAsync(attemptId);

            if (attempt == null)
                return NotFound();

            var task = await _taskService.GetTaskByIdAsync(attempt.LessonTaskId.Value);
            var vm = new TaskReviewViewModel
            {
                Attempt = attempt,
                Task = task
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ReviewTask(TaskReviewViewModel model)
        {
            var attempt = await _attemptService.GetUserTaskAttemptByIdAsync(model.Attempt.IdAttempt);

            if (attempt == null)
                return NotFound();

            attempt.IsCorrect = model.Attempt.IsCorrect;
            attempt.EarnedXp = model.Attempt.EarnedXp;
            attempt.Comment = model.Attempt.Comment;
            attempt.ReviewedAt = DateTime.Now;

            await _attemptService.UpdateUserTaskAttemptAsync(attempt.IdAttempt, attempt);

            var task = await _taskService.GetTaskByIdAsync(attempt.LessonTaskId.Value);

            return RedirectToAction(nameof(Solutions), new { lessonId = task.LessonId });
        }

        // ===================== Установка дедлайнов =====================
        public async Task<IActionResult> SetDeadline(int taskId)
        {
            var task = await _taskService.GetTaskByIdAsync(taskId);
            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> SetDeadline(int taskId, DateTime deadline)
        {
            var task = await _taskService.GetTaskByIdAsync(taskId);

            if (task == null)
                return NotFound();

            task.Deadline = deadline;

            await _taskService.UpdateTaskAsync(task.IdLessonTask, task);

            return RedirectToAction(nameof(Tasks), new { lessonId = task.LessonId });
        }

        // ===================== ПРОФИЛЬ ПРЕПОДАВАТЕЛЯ =====================
        public async Task<IActionResult> Profile()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.IdUser == userId);

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId) ?? new UserProgress
                {
                    UserId = userId,
                    Xp = 0,
                    Level = 1
                };

            var vm = new TeacherProfileViewModel
            {
                User = user,
                Profile = profile,
                Progress = progress,
                RegistrationDate = user?.RegistrationDate ?? DateTime.Now,
                TotalCourses = await _context.Courses.CountAsync(c => c.CreatedBy == userId),
                PublishedCourses = await _context.Courses.CountAsync(c => c.CreatedBy == userId && c.IsPublished == true),
                TotalStudents = await _context.StudentCourses
                    .Where(sc => sc.Course.CreatedBy == userId)
                    .Select(sc => sc.StudentId)
                    .Distinct()
                    .CountAsync(),
                ActiveStudents = 12,    
                TotalTasks = await _context.LessonTasks
                    .Where(t => t.Lesson.Module.Course.CreatedBy == userId)
                    .CountAsync(),
                PendingReviews = 5,        
                LastLogin = DateTime.Now
            };

            return View(vm);
        }

        // ===================== НАСТРОЙКИ ПРЕПОДАВАТЕЛЯ =====================
        public IActionResult Settings()
        {
            var vm = new TeacherSettingsViewModel
            {
                Theme = "light",
                Language = "ru",
                TimeZone = "Europe/Moscow"
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // Удаляем связанные данные
            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile != null)
                _context.UserProfiles.Remove(profile);

            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (progress != null)
                _context.UserProgresses.Remove(progress);

            var studentCourses = await _context.StudentCourses
                .Where(sc => sc.StudentId == userId)
                .ToListAsync();

            if (studentCourses.Any())
                _context.StudentCourses.RemoveRange(studentCourses);

            var attempts = await _context.UserTaskAttempts
                .Where(a => a.UserId == userId)
                .ToListAsync();

            if (attempts.Any())
                _context.UserTaskAttempts.RemoveRange(attempts);

            // Удаляем курсы преподавателя
            var courses = await _context.Courses
                .Where(c => c.CreatedBy == userId)
                .ToListAsync();

            if (courses.Any())
                _context.Courses.RemoveRange(courses);

            // Удаляем пользователя
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.IdUser == userId);

            if (user != null)
                _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            // Выход из системы
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Переход на авторизацию
            return RedirectToAction("Authorization", "Account");
        }

    }
}