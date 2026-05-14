using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using TeachCodIT.Models;

namespace TeachCodIT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var stats = GetPlatformStats();
            return View(stats);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        private (int CourseCount, int StudentCount, int LessonCount) GetPlatformStats()
        {
            int courseCount = 0;
            int studentCount = 0;
            int lessonCount = 0;

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Получаем количество курсов
                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Course WHERE IsPublished = 1", connection))
                {
                    courseCount = (int)command.ExecuteScalar();
                }

                // Получаем количество студентов
                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Role_ID = 3", connection))
                {
                    studentCount = (int)command.ExecuteScalar();
                }

                // Получаем количество уроков
                using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Lesson", connection))
                {
                    lessonCount = (int)command.ExecuteScalar();
                }
            }

            return (courseCount, studentCount, lessonCount);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}