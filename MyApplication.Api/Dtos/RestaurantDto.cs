namespace MyApplication.Api.Dtos;

public class RestaurantDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RestaurantTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
}
