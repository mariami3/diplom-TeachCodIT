using System.Net.Http.Json;
using TeachCodIT.Models;

namespace TeachCodIT.Services;

public class ModuleApiService
{
    private readonly HttpClient _httpClient;

    public ModuleApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("TeachCodIT-API");
    }

    public async Task<List<Module>> GetModulesByCourseAsync(int courseId)
    {
        return await _httpClient.GetFromJsonAsync<List<Module>>($"api/Modules/course/{courseId}") ?? new();
    }

    public async Task<Module?> GetModuleByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<Module>($"api/Modules/{id}");
    }

    public async Task<bool> CreateModuleAsync(Module module)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Modules", module);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateModuleAsync(int id, Module module)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/Modules/{id}", module);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteModuleAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/Modules/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ReorderModulesAsync(int courseId, List<int> orderedModuleIds)
    {
        var dto = new { CourseId = courseId, OrderedIds = orderedModuleIds };
        var response = await _httpClient.PostAsJsonAsync("api/Modules/reorder", dto);
        return response.IsSuccessStatusCode;
    }
}
