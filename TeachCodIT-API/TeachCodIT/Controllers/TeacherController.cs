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

        public TeacherController(

            TeachCodItContext context,
            CourseApiService courseService,
            ModuleApiService moduleService,
            LessonApiService lessonService,
            LessonTaskApiService taskService,
            TaskOptionApiService optionService,
            StudentCourseApiService studentCourseService,
            UserTaskAttemptApiService attemptService,
            UserApiService userApiService
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
        }

        // ===================== Курсы =====================
        public async Task<IActionResult> Dashboard()
        {
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var courses = await _context.Courses
                .Where(c => c.CreatedBy == teacherId)
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
            return RedirectToAction(nameof(Modules), new { courseId = module.CourseId });
        }

        public async Task<IActionResult> DeleteModule(int id, int courseId)
        {
            await _moduleService.DeleteModuleAsync(id);
            return RedirectToAction(nameof(Modules), new { courseId });
        }

        // ===================== Уроки =====================
        public async Task<IActionResult> Lessons(int moduleId)
        {
            var lessons = await _lessonService.GetLessonsByModuleAsync(moduleId);

            var module = await _moduleService.GetModuleByIdAsync(moduleId);

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
            return RedirectToAction(nameof(Lessons), new { moduleId = lesson.ModuleId });
        }

        public async Task<IActionResult> DeleteLesson(int id, int moduleId)
        {
            await _lessonService.DeleteLessonAsync(id);
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

        public IActionResult CreateTask(int lessonId)
        {
            ViewBag.LessonId = lessonId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(LessonTask task)
        {
            await _taskService.CreateTaskAsync(task);
            return RedirectToAction(nameof(Tasks), new { lessonId = task.LessonId });
        }

        public async Task<IActionResult> EditTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> EditTask(LessonTask task)
        {
            await _taskService.UpdateTaskAsync(task.IdLessonTask, task);
            return RedirectToAction(nameof(Tasks), new { lessonId = task.LessonId });
        }

        public async Task<IActionResult> DeleteTask(int id, int lessonId)
        {
            await _taskService.DeleteTaskAsync(id);
            return RedirectToAction(nameof(Tasks), new { lessonId });
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

            return View(students);
        }

        [HttpPost]
        public async Task<IActionResult> AssignCourse(int studentId, int courseId)
        {
            if (courseId <= 0)
            {
                TempData["Error"] = "Курс не выбран";
                return RedirectToAction(nameof(Students), new { courseId });
            }

            bool success = await _studentCourseService.EnrollAsync(studentId, courseId);

            if (!success)
            {
                TempData["Error"] = "Не удалось назначить курс (возможно студент уже записан или ошибка сервера)";
            }
            else
            {
                TempData["Success"] = "Студент успешно назначен на курс";
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
                .Where(sc => sc.CourseId == courseId)
                .Select(sc => new StudentCourseViewModel
                {
                    StudentId = sc.StudentId ?? 0,
                    OverallProgressPercent = sc.ProgressPercent ?? 0,

                    FullName =
                    sc.Student.UserProfiles.FirstOrDefault().FirstName + " " +
                    sc.Student.UserProfiles.FirstOrDefault().LastName,

                    TotalXP = sc.Student.UserProgresses.FirstOrDefault().Xp ?? 0,
                    StreakDays = sc.Student.UserProgresses.FirstOrDefault().StreakDays ?? 0,
                    LastActivityDate =
                        sc.Student.UserProgresses.FirstOrDefault().LastActivityDate.HasValue
                        ? sc.Student.UserProgresses.FirstOrDefault().LastActivityDate.Value.ToDateTime(TimeOnly.MinValue)
                        : null
                })
            .ToListAsync();

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.IdCourse == courseId);

            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = course?.Title;

            var analytics = new TeacherAnalyticsViewModel
            {
                AverageProgress = students.Any()
                    ? (int)students.Average(s => s.OverallProgressPercent)
                    : 0,

                TopStudents = students
                    .OrderByDescending(s => s.OverallProgressPercent)
                    .Take(3)
                    .ToList(),

                LowStudents = students
                    .OrderBy(s => s.OverallProgressPercent)
                    .Take(3)
                    .ToList()
            };

            return View(analytics);
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

            return RedirectToAction(nameof(Solutions), new { lessonId = attempt.LessonTaskId });
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

        
    }
}