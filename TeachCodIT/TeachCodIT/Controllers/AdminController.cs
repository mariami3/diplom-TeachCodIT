using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TeachCodIT.Models;
using TeachCodIT.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using OfficeOpenXml.Drawing.Chart.Style;
using OfficeOpenXml.Drawing.Chart;
using TeachCodIT.Services;
using Microsoft.AspNetCore.Authentication;

namespace TeachCodIT.Controllers
{
    [Authorize(Roles = "Админ")]
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly EmailService _emailService;

        public AdminController(IHttpClientFactory httpClientFactory, EmailService emailService)
        {
            _httpClient = httpClientFactory.CreateClient("TeachCodIT-API");
            _emailService = emailService;
        }

        // ====================== DASHBOARD (Аналитика) ======================
        public async Task<IActionResult> Index()
        {
            var dashboard = new AdminDashboardViewModel();

            // ================= USERS =================
            var usersResponse = await _httpClient.GetAsync("api/Users");
            List<User> allUsers = new();
            if (usersResponse.IsSuccessStatusCode)
            {
                allUsers = await usersResponse.Content.ReadFromJsonAsync<List<User>>() ?? new();
                dashboard.TotalUsers = allUsers.Count;
            }

            // ================= TOP STUDENTS =================
            var progressResponse = await _httpClient.GetAsync("api/UserProgresses"); 
            var allProgress = progressResponse.IsSuccessStatusCode
                ? await progressResponse.Content.ReadFromJsonAsync<List<UserProgress>>() ?? new()
                : new List<UserProgress>();

            var studentsOnly = allUsers.Where(u => u.RoleId == 3).ToList();

            dashboard.TopStudents = studentsOnly
                .Select(s =>
                {
                    var progress = allProgress.FirstOrDefault(p => p.UserId == s.IdUser);
                    return new TopStudent
                    {
                        UserId = s.IdUser,
                        Login = s.LoginUser,
                        XP = progress?.Xp ?? 0,
                        Level = progress?.Level ?? 1,
                        StreakDays = progress?.StreakDays ?? 0
                    };
                })
                .OrderByDescending(s => s.XP)
                .ThenByDescending(s => s.Level)
                .ThenBy(s => s.Login)
                .Take(10)
                .ToList();

            // ================= COURSES =================
            var coursesResponse = await _httpClient.GetAsync("api/Courses");
            var studentCoursesResponse = await _httpClient.GetAsync("api/StudentCourses");
            List<Course> courses = new();
            List<StudentCourse> enrollments = new();
            if (coursesResponse.IsSuccessStatusCode)
                courses = await coursesResponse.Content.ReadFromJsonAsync<List<Course>>() ?? new();
            if (studentCoursesResponse.IsSuccessStatusCode)
                enrollments = await studentCoursesResponse.Content.ReadFromJsonAsync<List<StudentCourse>>() ?? new();

            // ================= POPULAR COURSES =================
            dashboard.PopularCourses = courses
                .Select(c =>
                {
                    var courseEnrollments = enrollments.Where(e => e.CourseId == c.IdCourse).ToList();
                    return new PopularCourse
                    {
                        Id = c.IdCourse,
                        Title = c.Title,
                        Enrollments = courseEnrollments.Count,
                        AverageProgress = courseEnrollments.Any()
                            ? (int)courseEnrollments.Average(e => e.ProgressPercent ?? 0)
                            : 0,
                        GradientColor = c.GradientColor
                    };
                })
                .OrderByDescending(c => c.Enrollments)
                .Take(5)
                .ToList();

            // ================= REGISTRATION CHART (7 дней) =================
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            dashboard.RegistrationChartLabels = last7Days.Select(d => d.ToString("dd.MM")).ToList();
            dashboard.RegistrationChartData = last7Days
                .Select(day => allUsers.Count(u => u.RegistrationDate.Date == day.Date))
                .ToList();

            // ================= COURSE COMPLETION CHART =================
            var completed = enrollments.Where(e => e.ProgressPercent == 100).ToList();
            var topCoursesForChart = courses.Take(5).ToList();

            dashboard.CourseCompletionLabels = topCoursesForChart.Select(c => c.Title).ToList();
            dashboard.CourseCompletionData = topCoursesForChart
                .Select(c => completed.Count(e => e.CourseId == c.IdCourse))
                .ToList();

            // ================= AVERAGE LEARNING =================
            var avgProgress = enrollments.Any() ? enrollments.Average(e => e.ProgressPercent ?? 0) : 0;
            dashboard.AverageLearningTime = $"{(int)avgProgress}% курса";

            return View(dashboard);
        }

        // ====================== USERS ======================
        public async Task<IActionResult> Users()
        {
            var response = await _httpClient.GetAsync("api/Users");
            var users = response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<User>>()
                : new List<User>();
            return View(users);
        }

        public async Task<IActionResult> CreateUser()
        {
            var roles = await GetRolesAsync();
            ViewBag.Roles = new SelectList(roles, "IdRole", "NameRole");
            return View(new User());
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (!ModelState.IsValid)
            {
                var roles = await GetRolesAsync();
                ViewBag.Roles = new SelectList(roles, "IdRole", "NameRole", user.RoleId);
                return View(user);
            }

            var response = await _httpClient.PostAsJsonAsync("api/Users/register", user);

            if (response.IsSuccessStatusCode)
            {
                string roleName = user.RoleId switch
                {
                    1 => "Администратор",
                    2 => "Преподаватель",
                    3 => "Студент",
                    _ => "пользователь"
                };

                string subject = "Регистрация в TeachCodIT";
                string body = $@"
Здравствуйте, {user.LoginUser}!

Вы были зарегистрированы на платформе TeachCodIT.

Ваша роль: {roleName}

Логин: {user.LoginUser}
Пароль: {user.PasswordUser}

С уважением,
TeachCodIT
";

                await _emailService.SendEmailAsync(user.Email, subject, body);

                return RedirectToAction(nameof(Users));
            }

            ModelState.AddModelError("", await response.Content.ReadAsStringAsync());

            var rolesAgain = await GetRolesAsync();
            ViewBag.Roles = new SelectList(rolesAgain, "IdRole", "NameRole", user.RoleId);

            return View(user);
        }

        public async Task<IActionResult> EditUser(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await GetRolesAsync();
            ViewBag.Roles = new SelectList(roles, "IdRole", "NameRole", user.RoleId);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(User user)
        {
            if (!ModelState.IsValid)
            {
                var roles = await GetRolesAsync();
                ViewBag.Roles = new SelectList(roles, "IdRole", "NameRole", user.RoleId);
                return View(user);
            }

            var response = await _httpClient.PutAsJsonAsync($"api/Users/{user.IdUser}", user);
            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Users));

            ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            return View(user);
        }

        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await GetUserByIdAsync(id);
            return user == null ? NotFound() : View(user);
        }

        [HttpPost, ActionName("DeleteUser")]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            await _httpClient.DeleteAsync($"api/Users/{id}");
            return RedirectToAction(nameof(Users));
        }

        // ====================== ROLES ======================
        public async Task<IActionResult> Roles()
        {
            var roles = await GetRolesAsync();
            return View(roles);
        }

        public IActionResult CreateRole() => View(new Role());

        [HttpPost]
        public async Task<IActionResult> CreateRole(Role role)
        {
            if (ModelState.IsValid)
            {
                await _httpClient.PostAsJsonAsync("api/Roles", role);
                return RedirectToAction(nameof(Roles));
            }
            return View(role);
        }

        public async Task<IActionResult> EditRole(int id)
        {
            var role = await GetRoleByIdAsync(id);
            return role == null ? NotFound() : View(role);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(Role role)
        {
            if (ModelState.IsValid)
            {
                await _httpClient.PutAsJsonAsync($"api/Roles/{role.IdRole}", role);
                return RedirectToAction(nameof(Roles));
            }
            return View(role);
        }

        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await GetRoleByIdAsync(id);
            return role == null ? NotFound() : View(role);
        }

        [HttpPost, ActionName("DeleteRole")]
        public async Task<IActionResult> DeleteRoleConfirmed(int id)
        {
            await _httpClient.DeleteAsync($"api/Roles/{id}");
            return RedirectToAction(nameof(Roles));
        }

        // ====================== COURSES ======================
        public async Task<IActionResult> Courses()
        {
            var response = await _httpClient.GetAsync("api/Courses");
            var courses = response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<Course>>()
                : new List<Course>();
            return View(courses);
        }

        public async Task<IActionResult> CreateCourse()
        {
            var usersResponse = await _httpClient.GetAsync("api/Users");

            var users = usersResponse.IsSuccessStatusCode
                ? await usersResponse.Content.ReadFromJsonAsync<List<User>>()
                : new List<User>();

            var teachers = users
                .Where(u => u.RoleId == 2) 
                .ToList();

            ViewBag.Teachers = new SelectList(teachers, "IdUser", "LoginUser");

            return View(new Course());
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(Course course)
        {
            if (!ModelState.IsValid)
            {
                var usersResponse = await _httpClient.GetAsync("api/Users");

                var users = usersResponse.IsSuccessStatusCode
                    ? await usersResponse.Content.ReadFromJsonAsync<List<User>>()
                    : new List<User>();

                var teachers = users.Where(u => u.RoleId == 2).ToList();

                ViewBag.Teachers = new SelectList(teachers, "IdUser", "LoginUser", course.CreatedBy);

                return View(course); 
            }

            var response = await _httpClient.PostAsJsonAsync("api/Courses", course);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Courses)); 
            }

            ModelState.AddModelError("", await response.Content.ReadAsStringAsync());

            return View(course);
        }

        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await GetCourseByIdAsync(id);
            return course == null ? NotFound() : View(course);
        }

        [HttpPost]
        public async Task<IActionResult> EditCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                await _httpClient.PutAsJsonAsync($"api/Courses/{course.IdCourse}", course);
                return RedirectToAction(nameof(Courses));
            }
            return View(course);
        }

        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await GetCourseByIdAsync(id);
            return course == null ? NotFound() : View(course);
        }

        [HttpPost, ActionName("DeleteCourse")]
        public async Task<IActionResult> DeleteCourseConfirmed(int id)
        {
            await _httpClient.DeleteAsync($"api/Courses/{id}");
            return RedirectToAction(nameof(Courses));
        }

        // ====================== ACHIEVEMENTS ======================
        public async Task<IActionResult> Achievements()
        {
            var response = await _httpClient.GetAsync("api/Achievements");
            var achievements = response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<Achievement>>()
                : new List<Achievement>();
            return View(achievements);
        }

        public IActionResult CreateAchievement() => View(new Achievement());

        [HttpPost]
        public async Task<IActionResult> CreateAchievement(Achievement achievement)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync("api/Achievements", achievement);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Achievements));

                ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            }
            return View(achievement);
        }

        public async Task<IActionResult> EditAchievement(int id)
        {
            var achievement = await GetAchievementByIdAsync(id);
            if (achievement == null) return NotFound();
            return View(achievement);
        }

        [HttpPost]
        public async Task<IActionResult> EditAchievement(Achievement achievement)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Achievements/{achievement.IdAchievement}", achievement);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(Achievements));

                ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            }
            return View(achievement);
        }

        public async Task<IActionResult> DeleteAchievement(int id)
        {
            var achievement = await GetAchievementByIdAsync(id);
            if (achievement == null) return NotFound();
            return View(achievement);
        }

        [HttpPost, ActionName("DeleteAchievement")]
        public async Task<IActionResult> DeleteAchievementConfirmed(int id)
        {
            await _httpClient.DeleteAsync($"api/Achievements/{id}");
            return RedirectToAction(nameof(Achievements));
        }

        // ====================== DAILY QUESTS ======================
        public async Task<IActionResult> DailyQuests()
        {
            var response = await _httpClient.GetAsync("api/DailyQuests");
            var quests = response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<DailyQuest>>()
                : new List<DailyQuest>();
            return View(quests);
        }

        public IActionResult CreateDailyQuest() => View(new DailyQuest());

        [HttpPost]
        public async Task<IActionResult> CreateDailyQuest(DailyQuest quest)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PostAsJsonAsync("api/DailyQuests", quest);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(DailyQuests));

                ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            }
            return View(quest);
        }

        public async Task<IActionResult> EditDailyQuest(int id)
        {
            var quest = await GetDailyQuestByIdAsync(id);
            if (quest == null) return NotFound();
            return View(quest);
        }

        [HttpPost]
        public async Task<IActionResult> EditDailyQuest(DailyQuest quest)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync($"api/DailyQuests/{quest.IdQuest}", quest);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction(nameof(DailyQuests));

                ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
            }
            return View(quest);
        }

        public async Task<IActionResult> DeleteDailyQuest(int id)
        {
            var quest = await GetDailyQuestByIdAsync(id);
            if (quest == null) return NotFound();
            return View(quest);
        }

        [HttpPost, ActionName("DeleteDailyQuest")]
        public async Task<IActionResult> DeleteDailyQuestConfirmed(int id)
        {
            await _httpClient.DeleteAsync($"api/DailyQuests/{id}");
            return RedirectToAction(nameof(DailyQuests));
        }

        // ====================== Вспомогательные методы ======================
        private async Task<List<Role>> GetRolesAsync()
        {
            var response = await _httpClient.GetAsync("api/Roles");
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<List<Role>>()
                : new List<Role>();
        }

        private async Task<User?> GetUserByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Users/{id}");
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<User>()
                : null;
        }

        private async Task<Course?> GetCourseByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Courses/{id}");
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<Course>()
                : null;
        }

        private async Task<Role?> GetRoleByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Roles/{id}");
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<Role>()
                : null;
        }

        private async Task<Achievement?> GetAchievementByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/Achievements/{id}");
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<Achievement>()
                : null;
        }

        private async Task<DailyQuest?> GetDailyQuestByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/DailyQuests/{id}");
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<DailyQuest>()
                : null;
        }
        // ================= АДМИН ПРОФИЛЬ =================
        public async Task<IActionResult> Profile()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // Основные данные пользователя
            var userResponse = await _httpClient.GetAsync($"api/Users/{userId}");
            var user = userResponse.IsSuccessStatusCode
                ? await userResponse.Content.ReadFromJsonAsync<User>()
                : null;

            // Профиль пользователя (имя, фамилия, био)
            var profileResponse = await _httpClient.GetAsync($"api/UserProfiles/{userId}");
            var profile = profileResponse.IsSuccessStatusCode
                ? await profileResponse.Content.ReadFromJsonAsync<UserProfile>()
                : new UserProfile
                {
                    FirstName = user?.LoginUser ?? "Администратор",
                    Bio = "Администратор платформы TeachCodIT"
                };

            // Реальная статистика платформы
            var usersResp = await _httpClient.GetAsync("api/Users");
            int totalUsers = usersResp.IsSuccessStatusCode
                ? (await usersResp.Content.ReadFromJsonAsync<List<User>>())?.Count ?? 0 : 0;

            var coursesResp = await _httpClient.GetAsync("api/Courses");
            var allCourses = coursesResp.IsSuccessStatusCode
                ? await coursesResp.Content.ReadFromJsonAsync<List<Course>>() ?? new()
                : new List<Course>();
            int totalCourses = allCourses.Count;
            int publishedCourses = allCourses.Count(c => c.IsPublished == true);

            var achievementsResp = await _httpClient.GetAsync("api/Achievements");
            int totalAchievements = achievementsResp.IsSuccessStatusCode
                ? (await achievementsResp.Content.ReadFromJsonAsync<List<Achievement>>())?.Count ?? 0 : 0;

            var questsResp = await _httpClient.GetAsync("api/DailyQuests");
            int totalQuests = questsResp.IsSuccessStatusCode
                ? (await questsResp.Content.ReadFromJsonAsync<List<DailyQuest>>())?.Count ?? 0 : 0;

            var vm = new AdminProfileViewModel
            {
                User = user,
                Profile = profile,
                RegistrationDate = user?.RegistrationDate ?? DateTime.Now,
                TotalUsers = totalUsers,
                TotalCourses = totalCourses,
                TotalAchievements = totalAchievements,
                PublishedCourses = publishedCourses,
                TotalQuests = totalQuests,
                ActiveUsers = totalUsers > 0 ? (int)(totalUsers * 0.3) : 0,   // можно сделать точнее позже
                LastLogin = DateTime.Now
            };

            return View(vm);
        }

        // ================= ОБНОВЛЕНИЕ ПРОФИЛЯ =================
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string FirstName, string LastName, string Bio)
        {
            try
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                // Получаем текущий профиль
                var getResponse = await _httpClient.GetAsync($"api/UserProfiles/{userId}");
                UserProfile profile;

                if (getResponse.IsSuccessStatusCode)
                {
                    profile = await getResponse.Content.ReadFromJsonAsync<UserProfile>()
                              ?? new UserProfile { IdProfile = userId, UserId = userId };
                }
                else
                {
                    // Профиля нет — создаём новый
                    profile = new UserProfile
                    {
                        IdProfile = userId,
                        UserId = userId,
                        FirstName = FirstName?.Trim() ?? "Администратор",
                        LastName = LastName?.Trim() ?? "",
                        Bio = Bio?.Trim() ?? "Администратор платформы TeachCodIT"
                    };

                    // Создаём через POST
                    var postResponse = await _httpClient.PostAsJsonAsync("api/UserProfiles", profile);
                    if (postResponse.IsSuccessStatusCode)
                    {
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Не удалось создать профиль" });
                    }
                }

                // Обновляем существующий профиль
                profile.FirstName = FirstName?.Trim() ?? profile.FirstName;
                profile.LastName = LastName?.Trim() ?? profile.LastName;
                profile.Bio = Bio?.Trim() ?? profile.Bio;

                var putResponse = await _httpClient.PutAsJsonAsync($"api/UserProfiles/{userId}", profile);

                if (putResponse.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }

                var error = await putResponse.Content.ReadAsStringAsync();
                return Json(new { success = false, message = error });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ================= АДМИН НАСТРОЙКИ =================
        public IActionResult Settings()
        {
            var vm = new AdminSettingsViewModel
            {
                Theme = "light",
                Language = "ru",
                TimeZone = "Europe/Moscow"
            };

            return View(vm);
        }

        // ====================== Экспорт ======================
        public async Task<IActionResult> ExportPopularCourses()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Мариами Петриашвили");

            // === Получаем данные ===
            var coursesResponse = await _httpClient.GetAsync("api/Courses");
            var studentCoursesResponse = await _httpClient.GetAsync("api/StudentCourses");
            var usersResponse = await _httpClient.GetAsync("api/Users");

            List<Course> courses = new();
            List<StudentCourse> enrollments = new();
            List<User> allUsers = new();

            if (coursesResponse.IsSuccessStatusCode)
                courses = await coursesResponse.Content.ReadFromJsonAsync<List<Course>>() ?? new();

            if (studentCoursesResponse.IsSuccessStatusCode)
                enrollments = await studentCoursesResponse.Content.ReadFromJsonAsync<List<StudentCourse>>() ?? new();

            if (usersResponse.IsSuccessStatusCode)
                allUsers = await usersResponse.Content.ReadFromJsonAsync<List<User>>() ?? new();

            // Популярные курсы
            var popularCourses = courses
                .Select(c =>
                {
                    var courseEnrollments = enrollments.Where(e => e.CourseId == c.IdCourse).ToList();
                    return new
                    {
                        Title = c.Title,
                        Enrollments = courseEnrollments.Count,
                        AverageProgress = courseEnrollments.Any()
                            ? (int)courseEnrollments.Average(e => e.ProgressPercent ?? 0)
                            : 0
                    };
                })
                .OrderByDescending(c => c.Enrollments)
                .Take(10) 
                .ToList();

            // Данные для графика регистраций (последние 7 дней)
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            var registrationData = last7Days
                .Select(day => new
                {
                    Date = day.ToString("dd.MM"),
                    NewUsers = allUsers.Count(u => u.RegistrationDate.Date == day.Date)
                })
                .ToList();

          
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Популярные курсы");

            worksheet.Cells[1, 1].Value = "Популярные курсы";
            worksheet.Cells[1, 1, 1, 3].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[3, 1].Value = "Название курса";
            worksheet.Cells[3, 2].Value = "Количество студентов";
            worksheet.Cells[3, 3].Value = "Средний прогресс (%)";

            using (var range = worksheet.Cells[3, 1, 3, 3])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            int row = 4;
            foreach (var course in popularCourses)
            {
                worksheet.Cells[row, 1].Value = course.Title;
                worksheet.Cells[row, 2].Value = course.Enrollments;
                worksheet.Cells[row, 3].Value = course.AverageProgress;
                row++;
            }

            if (popularCourses.Any())
            {
                var barChart = worksheet.Drawings.AddChart("BarChart", eChartType.ColumnClustered);
                barChart.Title.Text = "Популярность курсов (количество студентов)";
                barChart.SetPosition(row + 2, 0, 1, 0);   
                barChart.SetSize(700, 400);

                var series = barChart.Series.Add(
                    ExcelRange.GetAddress(4, 2, row - 1, 2),    
                    ExcelRange.GetAddress(4, 1, row - 1, 1)      
                );

                series.Header = "Студенты";
                barChart.XAxis.Title.Text = "Курсы";
                barChart.YAxis.Title.Text = "Количество студентов";
                barChart.StyleManager.SetChartStyle(ePresetChartStyle.Column3dChartStyle9);
            }

            var regSheet = package.Workbook.Worksheets.Add("Регистрации пользователей");

            regSheet.Cells[1, 1].Value = "Дата";
            regSheet.Cells[1, 2].Value = "Новые пользователи";

            int regRow = 2;
            foreach (var item in registrationData)
            {
                regSheet.Cells[regRow, 1].Value = item.Date;
                regSheet.Cells[regRow, 2].Value = item.NewUsers;
                regRow++;
            }

            // Линейный график регистраций
            var lineChart = regSheet.Drawings.AddChart("RegistrationLineChart", eChartType.Line);
            lineChart.Title.Text = "Регистрации новых пользователей за последние 7 дней";
            lineChart.SetPosition(1, 0, 5, 0);
            lineChart.SetSize(800, 450);

            var lineSeries = lineChart.Series.Add(
                ExcelRange.GetAddress(2, 2, regRow - 1, 2),
                ExcelRange.GetAddress(2, 1, regRow - 1, 1)
            );

            lineSeries.Header = "Новые пользователи";
            lineChart.XAxis.Title.Text = "Дата";
            lineChart.YAxis.Title.Text = "Количество регистраций";
            lineChart.StyleManager.SetChartStyle(ePresetChartStyle.LineChartStyle8);

            // Автоподбор ширины колонок
            worksheet.Cells.AutoFitColumns();
            regSheet.Cells.AutoFitColumns();

            // === Сохраняем и возвращаем файл ===
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"TeachCodIT_Отчёт_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // ================= УДАЛЕНИЕ АККАУНТА АДМИНА =================
        [HttpPost]
        public async Task<IActionResult> DeleteMyAccount()
        {
            try
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                // Удаляем профиль
                await _httpClient.DeleteAsync($"api/UserProfiles/{userId}");

                // Удаляем пользователя
                var response = await _httpClient.DeleteAsync($"api/Users/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Ошибка удаления аккаунта"
                    });
                }

                // Выход из системы
                await HttpContext.SignOutAsync();

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("Authorization", "Account")
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}