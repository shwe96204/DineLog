using Shared.Models;

namespace Shared.Services;

public class RestaurantService
{
    private readonly List<RestaurantTypeItem> _restaurantTypes =
    [
        new()
        {
            Id = Guid.Parse("a1b2c3d4-e5f6-4789-a012-3456789abcde"),
            Name = "Fine Dining",
            Description = "Upscale restaurants with refined service and curated menus."
        },
        new()
        {
            Id = Guid.Parse("b2c3d4e5-f6a7-4890-b123-456789abcdef"),
            Name = "Casual Dining",
            Description = "Relaxed full-service restaurants for everyday meals."
        },
        new()
        {
            Id = Guid.Parse("c3d4e5f6-a7b8-4901-c234-56789abcdef0"),
            Name = "Cafe",
            Description = "Coffee shops, bakeries, and light-meal spots."
        },
        new()
        {
            Id = Guid.Parse("d4e5f6a7-b8c9-4012-d345-6789abcdef01"),
            Name = "Fast Food",
            Description = "Quick-service chains and counter-order venues."
        },
        new()
        {
            Id = Guid.Parse("e5f6a7b8-c9d0-4123-e456-789abcdef012"),
            Name = "Street Food",
            Description = "Market stalls, food trucks, and local street vendors."
        },
        new()
        {
            Id = Guid.Parse("f6a7b8c9-d0e1-4234-f567-89abcdef0123"),
            Name = "Bar & Pub",
            Description = "Drinks-focused venues with bar bites or pub menus."
        },
        new()
        {
            Id = Guid.Parse("a7b8c9d0-e1f2-4345-a678-9abcdef01234"),
            Name = "Buffet",
            Description = "Self-service or all-you-can-eat dining formats."
        },
        new()
        {
            Id = Guid.Parse("b8c9d0e1-f2a3-4456-b789-abcdef012345"),
            Name = "Food Court",
            Description = "Shared dining halls with multiple vendors."
        }
    ];

    private readonly List<RestaurantItem> _restaurants =
    [
        new()
        {
            Id = Guid.Parse("11111111-1111-4111-8111-111111111101"),
            RestaurantTypeId = Guid.Parse("a1b2c3d4-e5f6-4789-a012-3456789abcde"),
            Name = "Lemon Basil Kitchen",
            Country = "Myanmar",
            City = "Yangon",
            Address = "12 Parami Road, Hlaing Township",
            Description = "Modern Asian fusion with seasonal herbs and citrus-forward plates.",
            Image = "image/restaurants/lemon-basil.jpg",
            WebsiteUrl = "https://example.com/lemon-basil"
        },
        new()
        {
            Id = Guid.Parse("22222222-2222-4222-8222-222222222202"),
            RestaurantTypeId = Guid.Parse("f6a7b8c9-d0e1-4234-f567-89abcdef0123"),
            Name = "Rangoon Rooftable",
            Country = "Myanmar",
            City = "Yangon",
            Address = "88 Strand Road, Downtown",
            Description = "Rooftop bar with skyline views and shareable small plates.",
            Image = "image/restaurants/rangoon-roof.jpg",
            WebsiteUrl = "https://example.com/rangoon-roof"
        },
        new()
        {
            Id = Guid.Parse("33333333-3333-4333-8333-333333333303"),
            RestaurantTypeId = Guid.Parse("c3d4e5f6-a7b8-4901-c234-56789abcdef0"),
            Name = "Morning Brew Lab",
            Country = "Myanmar",
            City = "Mandalay",
            Address = "4 62nd Street, Chan Aye Thar Zan",
            Description = "Specialty coffee, pastries, and brunch bowls in a bright corner cafe.",
            Image = "image/restaurants/morning-brew.jpg",
            WebsiteUrl = ""
        }
    ];

    public IReadOnlyList<RestaurantTypeItem> GetRestaurantTypes() =>
        _restaurantTypes.OrderBy(x => x.Name).ToList();

    public RestaurantTypeItem? GetRestaurantType(Guid id) =>
        _restaurantTypes.FirstOrDefault(x => x.Id == id);

    public string GetRestaurantTypeName(Guid restaurantTypeId) =>
        GetRestaurantType(restaurantTypeId)?.Name ?? "Unknown";

    public IReadOnlyList<RestaurantItem> GetRestaurants() =>
        _restaurants.OrderBy(x => x.Name).ToList();

    public RestaurantItem? GetRestaurant(Guid id) =>
        _restaurants.FirstOrDefault(x => x.Id == id);

    public void SaveRestaurant(RestaurantItem restaurant)
    {
        restaurant.Name = restaurant.Name.Trim();
        restaurant.Country = restaurant.Country.Trim();
        restaurant.City = restaurant.City.Trim();
        restaurant.Address = restaurant.Address.Trim();
        restaurant.Description = restaurant.Description.Trim();
        restaurant.Image = restaurant.Image.Trim();
        restaurant.WebsiteUrl = restaurant.WebsiteUrl.Trim();

        var existing = _restaurants.FirstOrDefault(x => x.Id == restaurant.Id);
        if (existing is null)
        {
            restaurant.Id = restaurant.Id == Guid.Empty ? Guid.NewGuid() : restaurant.Id;
            _restaurants.Add(new RestaurantItem
            {
                Id = restaurant.Id,
                RestaurantTypeId = restaurant.RestaurantTypeId,
                Name = restaurant.Name,
                Country = restaurant.Country,
                City = restaurant.City,
                Address = restaurant.Address,
                Description = restaurant.Description,
                Image = restaurant.Image,
                WebsiteUrl = restaurant.WebsiteUrl
            });
            return;
        }

        existing.RestaurantTypeId = restaurant.RestaurantTypeId;
        existing.Name = restaurant.Name;
        existing.Country = restaurant.Country;
        existing.City = restaurant.City;
        existing.Address = restaurant.Address;
        existing.Description = restaurant.Description;
        existing.Image = restaurant.Image;
        existing.WebsiteUrl = restaurant.WebsiteUrl;
    }

    public void DeleteRestaurant(Guid id)
    {
        var restaurant = _restaurants.FirstOrDefault(x => x.Id == id);
        if (restaurant is not null)
        {
            _restaurants.Remove(restaurant);
        }
    }
}
