using System.Net.Http.Json;
using TeachCodIT.Models;

namespace TeachCodIT.Services
{
    public class RoleApiService
    {
        private readonly HttpClient _httpClient;

        public RoleApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("TeachCodIT-API");
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Role>>("api/Roles") ?? new();
        }

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Role>($"api/Roles/{id}");
        }

        public async Task<bool> CreateRoleAsync(Role role)
        {
            var resp = await _httpClient.PostAsJsonAsync("api/Roles", role);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateRoleAsync(int id, Role role)
        {
            var resp = await _httpClient.PutAsJsonAsync($"api/Roles/{id}", role);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var resp = await _httpClient.DeleteAsync($"api/Roles/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}