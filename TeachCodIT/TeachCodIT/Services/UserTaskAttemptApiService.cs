using System.Net.Http.Json;
using TeachCodIT.Models;

namespace TeachCodIT.Services;

public class UserTaskAttemptApiService
{
    private readonly HttpClient _httpClient;

    public UserTaskAttemptApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("TeachCodIT-API");
    }

    public async Task<UserTaskAttempt?> GetUserTaskAttemptByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<UserTaskAttempt>($"api/UserTaskAttempts/{id}");
    }

    public async Task<List<UserTaskAttempt>> GetAttemptsByLessonAsync(int lessonId)
    {
        return await _httpClient.GetFromJsonAsync<List<UserTaskAttempt>>($"api/UserTaskAttempts/lesson/{lessonId}") ?? new();
    }

    public async Task<bool> UpdateUserTaskAttemptAsync(int id, UserTaskAttempt attempt)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/UserTaskAttempts/{id}", attempt);
        return response.IsSuccessStatusCode;
    }
}
