using System.Net.Http.Json;
using TeachCodIT.Models;

namespace TeachCodIT.Services;

public class UserProfileApiService
{
    private readonly HttpClient _httpClient;

    public UserProfileApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("TeachCodIT-API");
    }

    /// <summary>
    /// Получить профиль пользователя по ID пользователя
    /// </summary>
    public async Task<UserProfile?> GetUserProfileAsync(int userId)
    {
        return await _httpClient.GetFromJsonAsync<UserProfile>($"api/UserProfiles/user/{userId}");
    }

    /// <summary>
    /// Создать/обновить профиль (часто используется upsert-логика на бэкенде)
    /// </summary>
    public async Task<bool> UpdateUserProfileAsync(int userId, UserProfile profile)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/UserProfiles/user/{userId}", profile);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Загрузить аватар (multipart/form-data)
    /// </summary>
    public async Task<bool> UploadAvatarAsync(int userId, Stream imageStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(imageStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg"); // или другой тип
        content.Add(fileContent, "avatar", fileName);

        var response = await _httpClient.PostAsync($"api/UserProfiles/{userId}/avatar", content);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Получить только URL аватара (если нужно быстро показать)
    /// </summary>
    public async Task<string?> GetAvatarUrlAsync(int userId)
    {
        var response = await _httpClient.GetAsync($"api/UserProfiles/{userId}/avatar-url");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Обновить только био / имя / фамилию (патч)
    /// </summary>
    public async Task<bool> PatchProfileAsync(int userId, object patchData)
    {
        var response = await _httpClient.PatchAsJsonAsync($"api/UserProfiles/{userId}", patchData);
        return response.IsSuccessStatusCode;
    }
}