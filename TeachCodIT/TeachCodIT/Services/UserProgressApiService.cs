using System.Net.Http.Json;
using TeachCodIT.Models;

namespace TeachCodIT.Services;

public class UserProgressApiService
{
    private readonly HttpClient _httpClient;

    public UserProgressApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("TeachCodIT-API");
    }

    public async Task<UserProgress?> GetProgressAsync(int userId)
    {
        return await _httpClient.GetFromJsonAsync<UserProgress>($"api/UserProgress/{userId}");
    }

    public async Task<bool> AddExperienceAsync(int userId, int xpToAdd)
    {
        var dto = new { XpToAdd = xpToAdd };
        var response = await _httpClient.PostAsJsonAsync($"api/UserProgress/{userId}/add-xp", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateStreakAsync(int userId)
    {
        var response = await _httpClient.PostAsync($"api/UserProgress/{userId}/update-streak", null);
        return response.IsSuccessStatusCode;
    }
}