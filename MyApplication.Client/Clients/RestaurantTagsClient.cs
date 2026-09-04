using System.Net.Http.Json;
using Shared.Models;

namespace MyApplication.Client.Clients;

public class RestaurantTagsClient(HttpClient httpClient)
{
    public async Task<RestaurantTagItem[]> GetTagsAsync(Guid userId) =>
        await httpClient.GetFromJsonAsync<RestaurantTagItem[]>($"tags?userId={userId}") ?? [];

    public async Task<RestaurantTagItem?> GetTagAsync(Guid id, Guid userId) =>
        await httpClient.GetFromJsonAsync<RestaurantTagItem>($"tags/{id}?userId={userId}");

    public async Task AddTagAsync(RestaurantTagItem tag) =>
        await httpClient.PostAsJsonAsync("tags", tag);

    public async Task UpdateTagAsync(RestaurantTagItem tag) =>
        await httpClient.PutAsJsonAsync($"tags/{tag.Id}", tag);

    public async Task DeleteTagAsync(Guid id, Guid userId) =>
        await httpClient.DeleteAsync($"tags/{id}?userId={userId}");
}
