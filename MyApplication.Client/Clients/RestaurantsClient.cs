using System.Net.Http.Json;
using Shared.Models;

namespace MyApplication.Client.Clients;

public class RestaurantsClient(HttpClient httpClient)
{
    public async Task<RestaurantItem[]> GetRestaurantsAsync(Guid userId) =>
        await httpClient.GetFromJsonAsync<RestaurantItem[]>($"restaurants?userId={userId}") ?? [];

    public async Task<RestaurantItem> GetRestaurantAsync(Guid id, Guid userId) =>
        await httpClient.GetFromJsonAsync<RestaurantItem>($"restaurants/{id}?userId={userId}")
        ?? throw new Exception($"Restaurant with id {id} was not found.");

    public async Task AddRestaurantAsync(RestaurantItem restaurant) =>
        await httpClient.PostAsJsonAsync("restaurants", restaurant);

    public async Task UpdateRestaurantAsync(RestaurantItem restaurant) =>
        await httpClient.PutAsJsonAsync($"restaurants/{restaurant.Id}", restaurant);

    public async Task DeleteRestaurantAsync(Guid id, Guid userId) =>
        await httpClient.DeleteAsync($"restaurants/{id}?userId={userId}");
}
