using System.Net.Http.Json;
using TeachCodIT.Models;

namespace TeachCodIT.Services
{
    public class TaskOptionApiService
    {
        private readonly HttpClient _httpClient;

        public TaskOptionApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("TeachCodIT-API");
        }

        public async Task<List<TaskOption>> GetOptionsByTaskAsync(int taskId)
        {
            return await _httpClient.GetFromJsonAsync<List<TaskOption>>($"api/TaskOptions/task/{taskId}") ?? new();
        }

        public async Task<bool> CreateOptionAsync(TaskOption option)
        {
            var response = await _httpClient.PostAsJsonAsync("api/TaskOptions", option);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateOptionAsync(int id, TaskOption option)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/TaskOptions/{id}", option);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteOptionAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/TaskOptions/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
