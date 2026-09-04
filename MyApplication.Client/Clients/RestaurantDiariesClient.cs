using System.Net.Http.Json;
using Shared.Models;

namespace MyApplication.Client.Clients;

public class RestaurantDiariesClient(HttpClient httpClient)
{
    public async Task<RestaurantDiaryItem[]> GetRestaurantDiariesAsync(Guid userId, Guid diaryId) =>
        await httpClient.GetFromJsonAsync<RestaurantDiaryItem[]>($"restaurant-diaries?userId={userId}&diaryId={diaryId}") ?? [];

    public async Task<RestaurantDiaryItem?> GetRestaurantDiaryAsync(Guid id, Guid userId) =>
        await httpClient.GetFromJsonAsync<RestaurantDiaryItem>($"restaurant-diaries/{id}?userId={userId}");

    public async Task<RestaurantDiaryItem?> AddRestaurantDiaryAsync(RestaurantDiaryItem entry)
    {
        var response = await httpClient.PostAsJsonAsync("restaurant-diaries", entry);
        return await response.Content.ReadFromJsonAsync<RestaurantDiaryItem>();
    }

    public async Task<RestaurantDiaryItem?> UpdateRestaurantDiaryAsync(RestaurantDiaryItem entry)
    {
        var response = await httpClient.PutAsJsonAsync($"restaurant-diaries/{entry.Id}", entry);
        return await response.Content.ReadFromJsonAsync<RestaurantDiaryItem>();
    }

    public async Task DeleteRestaurantDiaryAsync(Guid id, Guid userId) =>
        await httpClient.DeleteAsync($"restaurant-diaries/{id}?userId={userId}");
}
