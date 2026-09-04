namespace MyApplication.Api.Entities;

public class RestaurantType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<Restaurant> Restaurants { get; set; } = [];
}
