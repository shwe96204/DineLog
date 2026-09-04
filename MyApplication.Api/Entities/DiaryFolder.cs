namespace MyApplication.Api.Entities;

public class DiaryFolder
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DiaryName { get; set; } = string.Empty;
    public string Color { get; set; } = "#d19044";
    public DateTime DateCreated { get; set; }
    public string CoverImage { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public AppUser? User { get; set; }
    public ICollection<RestaurantDiary> RestaurantDiaries { get; set; } = [];
}
