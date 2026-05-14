using System.Net.Http.Json;
using TeachCodIT.Models;

namespace TeachCodIT.Services;

public class DailyQuestApiService
{
    private readonly HttpClient _httpClient;

    public DailyQuestApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("TeachCodIT-API");
    }

    /// <summary>
    /// Получить все доступные ежедневные квесты (обычно админ-панель)
    /// </summary>
    public async Task<List<DailyQuest>> GetAllDailyQuestsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<DailyQuest>>("api/DailyQuests") ?? new();
    }

    /// <summary>
    /// Получить квест по идентификатору
    /// </summary>
    public async Task<DailyQuest?> GetDailyQuestByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<DailyQuest>($"api/DailyQuests/{id}");
    }

    /// <summary>
    /// Создать новый ежедневный квест (обычно только администратор)
    /// </summary>
    public async Task<bool> CreateDailyQuestAsync(DailyQuest quest)
    {
        var response = await _httpClient.PostAsJsonAsync("api/DailyQuests", quest);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Обновить существующий квест
    /// </summary>
    public async Task<bool> UpdateDailyQuestAsync(int id, DailyQuest quest)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/DailyQuests/{id}", quest);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Удалить квест
    /// </summary>
    public async Task<bool> DeleteDailyQuestAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/DailyQuests/{id}");
        return response.IsSuccessStatusCode;
    }

    // Опционально: получить активные квесты на сегодня (если бэкенд сам генерирует/фильтрует)
    public async Task<List<DailyQuest>> GetActiveDailyQuestsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<DailyQuest>>("api/DailyQuests/active") ?? new();
    }
}