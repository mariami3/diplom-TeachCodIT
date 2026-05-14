using System.Net.Http.Json;
using TeachCodIT.Models;

namespace TeachCodIT.Services;

public class UserDailyQuestApiService
{
    private readonly HttpClient _httpClient;

    public UserDailyQuestApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("TeachCodIT-API");
    }

    /// <summary>
    /// Получить все ежедневные квесты пользователя за конкретный день (или сегодня)
    /// </summary>
    public async Task<List<UserDailyQuest>> GetUserDailyQuestsAsync(int userId, DateOnly? date = null)
    {
        string url = $"api/UserDailyQuests/user/{userId}";

        if (date.HasValue)
        {
            url += $"?date={date.Value:yyyy-MM-dd}";
        }
        else
        {
            // по умолчанию — сегодня
            url += $"?date={DateOnly.FromDateTime(DateTime.Today):yyyy-MM-dd}";
        }

        return await _httpClient.GetFromJsonAsync<List<UserDailyQuest>>(url) ?? new();
    }

    /// <summary>
    /// Получить сегодняшние квесты пользователя (удобный метод)
    /// </summary>
    public async Task<List<UserDailyQuest>> GetTodayUserQuestsAsync(int userId)
    {
        return await GetUserDailyQuestsAsync(userId, DateOnly.FromDateTime(DateTime.Today));
    }

    /// <summary>
    /// Отметить квест как выполненный → обычно возвращает обновлённый объект + начисленный XP
    /// </summary>
    public async Task<UserDailyQuest?> CompleteQuestAsync(int userId, int questId)
    {
        var dto = new { UserId = userId, QuestId = questId };
        var response = await _httpClient.PostAsJsonAsync("api/UserDailyQuests/complete", dto);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<UserDailyQuest>();
    }

    /// <summary>
    /// Проверить, сколько квестов пользователь выполнил сегодня
    /// </summary>
    public async Task<int> GetTodayCompletedCountAsync(int userId)
    {
        var response = await _httpClient.GetAsync($"api/UserDailyQuests/user/{userId}/today/completed-count");
        if (!response.IsSuccessStatusCode) return 0;

        var content = await response.Content.ReadAsStringAsync();
        return int.TryParse(content, out int count) ? count : 0;
    }

    /// <summary>
    /// Получить квесты за последние N дней (для статистики / календаря)
    /// </summary>
    public async Task<List<UserDailyQuest>> GetLastDaysQuestsAsync(int userId, int days = 7)
    {
        return await _httpClient.GetFromJsonAsync<List<UserDailyQuest>>(
            $"api/UserDailyQuests/user/{userId}/last-days?days={days}") ?? new();
    }
}