using BambooServiceApi.Models;
using System.Net.Http.Json;

namespace BambooServiceApi.Clients;

public interface IBambooServiceClient
{
    Task<List<BambooDto>> GetAllBambooAsync();
    Task<BambooDto?> GetBambooByIdAsync(int id);
    Task<BambooDto> CreateBambooAsync(BambooDto bamboo);
    Task<BambooDto?> UpdateBambooAsync(int id, BambooDto bamboo);
    Task<bool> DeleteBambooAsync(int id);
}

public class BambooServiceClient : IBambooServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public BambooServiceClient(HttpClient httpClient, string baseUrl = "https://localhost:5002")
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
    }

    public async Task<List<BambooDto>> GetAllBambooAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/bamboo");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<BambooDto>>() ?? new List<BambooDto>();
    }

    public async Task<BambooDto?> GetBambooByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/bamboo/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BambooDto>();
    }

    public async Task<BambooDto> CreateBambooAsync(BambooDto bamboo)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/bamboo", bamboo);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BambooDto>() ?? bamboo;
    }

    public async Task<BambooDto?> UpdateBambooAsync(int id, BambooDto bamboo)
    {
        var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/api/bamboo/{id}", bamboo);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BambooDto>();
    }

    public async Task<bool> DeleteBambooAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/bamboo/{id}");
        return response.IsSuccessStatusCode;
    }
}
