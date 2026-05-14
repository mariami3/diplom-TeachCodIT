using System.Net.Http.Json;
using TeachCodIT.Models;

namespace TeachCodIT.Services;

public class CourseApiService
{
    private readonly HttpClient _http;

    public CourseApiService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("TeachCodIT-API");
    }

    public async Task<List<Course>> GetPublishedCoursesAsync()
    {
        return await _http.GetFromJsonAsync<List<Course>>("api/Courses/published") ?? new();
    }

    public async Task<List<Course>> GetAllCoursesAsync()
    {
        return await _http.GetFromJsonAsync<List<Course>>("api/Courses") ?? new();
    }

    public async Task<Course?> GetCourseByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<Course>($"api/Courses/{id}");
    }

    public async Task<bool> CreateCourseAsync(Course course)
    {
        var response = await _http.PostAsJsonAsync("api/Courses", course);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCourseAsync(int id, Course course)
    {
        var response = await _http.PutAsJsonAsync($"api/Courses/{id}", course);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> PublishCourseAsync(int id)
    {
        var response = await _http.PatchAsync($"api/Courses/{id}/publish", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCourseAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/Courses/{id}");
        return response.IsSuccessStatusCode;
    }
}