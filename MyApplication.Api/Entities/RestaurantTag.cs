namespace MyApplication.Api.Entities;

public class RestaurantTag
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public AppUser? User { get; set; }
}
