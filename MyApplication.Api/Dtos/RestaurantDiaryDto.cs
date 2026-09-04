namespace MyApplication.Api.Dtos;

public class RestaurantDiaryDto
{
    public Guid Id { get; set; }
    public Guid DiaryId { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid RestaurantTypeId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public string RestaurantCity { get; set; } = string.Empty;
    public string RestaurantCountry { get; set; } = string.Empty;
    public int OverallRating { get; set; }
    public int TasteScore { get; set; }
    public int ServiceScore { get; set; }
    public int CleanlinessScore { get; set; }
    public int AmbienceScore { get; set; }
    public DateTime? VisitedDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string DetailImage { get; set; } = string.Empty;
    public List<Guid> TagIds { get; set; } = [];
    public List<string> GalleryImages { get; set; } = [];
}
