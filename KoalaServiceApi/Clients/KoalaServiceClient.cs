using KoalaServiceApi.Models;
using System.Net.Http.Json;

namespace KoalaServiceApi.Clients;

public interface IKoalaServiceClient
{
    Task<List<KoalaDto>> GetAllKoalasAsync();
    Task<KoalaDto?> GetKoalaByIdAsync(int id);
    Task<KoalaDto> CreateKoalaAsync(KoalaDto koala);
    Task<KoalaDto?> UpdateKoalaAsync(int id, KoalaDto koala);
    Task<bool> DeleteKoalaAsync(int id);
}

public class KoalaServiceClient : IKoalaServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public KoalaServiceClient(HttpClient httpClient, string baseUrl = "https://localhost:5001")
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
    }

    public async Task<List<KoalaDto>> GetAllKoalasAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/koalas");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<KoalaDto>>() ?? new List<KoalaDto>();
    }

    public async Task<KoalaDto?> GetKoalaByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/koalas/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<KoalaDto>();
    }

    public async Task<KoalaDto> CreateKoalaAsync(KoalaDto koala)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/koalas", koala);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<KoalaDto>() ?? koala;
    }

    public async Task<KoalaDto?> UpdateKoalaAsync(int id, KoalaDto koala)
    {
        var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/api/koalas/{id}", koala);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<KoalaDto>();
    }

    public async Task<bool> DeleteKoalaAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/koalas/{id}");
        return response.IsSuccessStatusCode;
    }
}
