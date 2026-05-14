using System.Net.Http.Json;
using TeachCodIT.Models;

namespace TeachCodIT.Services;

public class UserAchievementApiService
{
    private readonly HttpClient _httpClient;

    public UserAchievementApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("TeachCodIT-API");
    }

    public async Task<List<UserAchievement>> GetUserAchievementsAsync(int userId)
    {
        return await _httpClient.GetFromJsonAsync<List<UserAchievement>>(
            $"api/UserAchievements/user/{userId}") ?? new();
    }

    public async Task<List<Achievement>> GetAvailableAchievementsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Achievement>>("api/Achievements") ?? new();
    }
}