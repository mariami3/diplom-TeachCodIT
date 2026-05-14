using System.Net.Http.Json;
using TeachCodIT.Models;
using TeachCodIT.Models.ViewModels;

namespace TeachCodIT.Services;

public class StudentCourseApiService
{
    private readonly HttpClient _http;

    public StudentCourseApiService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("TeachCodIT-API");
    }

    public async Task<bool> EnrollAsync(int studentId, int courseId)
    {
        var dto = new { StudentId = studentId, CourseId = courseId };
        var response = await _http.PostAsJsonAsync("api/StudentCourses/enroll", dto);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Ошибка при записи студента: {response.StatusCode} → {errorContent}");
        }

        return response.IsSuccessStatusCode;
    }

    public async Task<List<StudentCourse>> GetEnrolledCoursesAsync(int studentId)
    {
        return await _http.GetFromJsonAsync<List<StudentCourse>>($"api/StudentCourses/student/{studentId}") ?? new();
    }

    public async Task<StudentCourse?> GetEnrollmentAsync(int studentId, int courseId)
    {
        return await _http.GetFromJsonAsync<StudentCourse>($"api/StudentCourses/{studentId}/{courseId}");
    }

    public async Task<bool> UpdateProgressAsync(int studentId, int courseId, int progressPercent)
    {
        var dto = new { ProgressPercent = progressPercent };
        var response = await _http.PatchAsJsonAsync($"api/StudentCourses/{studentId}/{courseId}/progress", dto);
        return response.IsSuccessStatusCode;
 
    }
    public async Task<List<StudentCourseViewModel>> GetStudentsByCourseAsync(int courseId)
    {
        var enrollments = await _http.GetFromJsonAsync<List<StudentCourse>>(
            $"api/studentcourses/course/{courseId}") ?? new List<StudentCourse>();

        var result = new List<StudentCourseViewModel>();

        foreach (var sc in enrollments)
        {
            var user = sc.Student ?? new User(); // защита от null

            UserProfile? profile = null;
            UserProgress? progress = null;

            try
            {
                profile = await _http.GetFromJsonAsync<UserProfile?>(
                    $"api/userprofile/{user.IdUser}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // просто игнорируем — профиля нет
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки профиля {user.IdUser}: {ex.Message}");
            }

            try
            {
                progress = await _http.GetFromJsonAsync<UserProgress?>(
                    $"api/userprogress/{user.IdUser}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // игнорируем
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки прогресса {user.IdUser}: {ex.Message}");
            }

            result.Add(new StudentCourseViewModel
            {
                StudentId = user.IdUser,
                FullName = !string.IsNullOrEmpty(profile?.FirstName) || !string.IsNullOrEmpty(profile?.LastName)
                    ? $"{profile?.FirstName ?? ""} {profile?.LastName ?? ""}".Trim()
                    : user.LoginUser ?? "Студент без имени",
                OverallProgressPercent = sc.ProgressPercent ?? 0,
                TotalXP = progress?.Xp ?? 0,
                StreakDays = progress?.StreakDays ?? 0,
                User = user
            });
        }

        return result;
    }

    public async Task<bool> UnenrollAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/StudentCourses/{id}");
        return response.IsSuccessStatusCode;
    }
}