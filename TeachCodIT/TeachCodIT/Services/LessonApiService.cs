using System.Net.Http.Json;
using TeachCodIT.Models;

namespace TeachCodIT.Services
{
    public class LessonApiService
    {
        private readonly HttpClient _httpClient;

        public LessonApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("TeachCodIT-API");
        }

        public async Task<List<Lesson>> GetLessonsByModuleAsync(int moduleId)
        {
            return await _httpClient.GetFromJsonAsync<List<Lesson>>($"api/Lessons/module/{moduleId}") ?? new();
        }

        public async Task<Lesson?> GetLessonByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Lesson>($"api/Lessons/{id}");
        }

        public async Task<bool> CreateLessonAsync(Lesson lesson)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Lessons", lesson);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateLessonAsync(int id, Lesson lesson)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Lessons/{id}", lesson);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteLessonAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Lessons/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}