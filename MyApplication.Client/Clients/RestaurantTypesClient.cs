using System.Net.Http.Json;
using Shared.Models;

namespace MyApplication.Client.Clients;

public class RestaurantTypesClient(HttpClient httpClient)
{
    public async Task<RestaurantTypeItem[]> GetRestaurantTypesAsync() =>
        await httpClient.GetFromJsonAsync<RestaurantTypeItem[]>("restaurant-types") ?? [];
}
