using System.Net.Http.Json;
using TeachCodIT.Models;

namespace TeachCodIT.Services
{
    public class LessonTaskApiService
    {
        private readonly HttpClient _httpClient;

        public LessonTaskApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("TeachCodIT-API");
        }

        public async Task<List<LessonTask>> GetTasksByLessonAsync(int lessonId)
        {
            return await _httpClient.GetFromJsonAsync<List<LessonTask>>($"api/LessonTasks/lesson/{lessonId}") ?? new();
        }

        public async Task<LessonTask?> GetTaskByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<LessonTask>($"api/LessonTasks/{id}");
        }

        public async Task<bool> CreateTaskAsync(LessonTask task)
        {
            var response = await _httpClient.PostAsJsonAsync("api/LessonTasks", task);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTaskAsync(int id, LessonTask task)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/LessonTasks/{id}", task);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/LessonTasks/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}