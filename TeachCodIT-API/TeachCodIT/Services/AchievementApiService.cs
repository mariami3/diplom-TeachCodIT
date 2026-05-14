using System.Net.Http.Json;
using TeachCodIT.Models;

namespace TeachCodIT.Services
{
    public class AchievementApiService
    {
        private readonly HttpClient _httpClient;

        public AchievementApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("TeachCodIT-API");
        }

        public async Task<List<Achievement>> GetAchievementsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Achievement>>("api/Achievements");
        }
    }
}