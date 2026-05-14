using System.Net.Http.Json;
using TeachCodIT.Models;

namespace TeachCodIT.Services
{
    public class UserApiService
    {
        private readonly HttpClient _httpClient;

        public UserApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("TeachCodIT-API");
        }

        public async Task<HttpResponseMessage> Register(User user)
        {
            return await _httpClient.PostAsJsonAsync("api/Users/register", user);
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<User>>("api/Users");
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<User>($"api/Users/{id}");
        }

        public async Task<List<User>> GetStudentsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<User>>("api/Users/students") ?? new();
        }

        public async Task RegisterAsync(User user)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Users/register", user);
            response.EnsureSuccessStatusCode();
        }

        public async Task<User?> AuthenticateAsync(object loginModel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Users/authenticate", loginModel);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<User>();
        }

        public async Task DeleteUserAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Users/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}