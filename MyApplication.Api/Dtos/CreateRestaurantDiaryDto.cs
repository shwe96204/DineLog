namespace MyApplication.Api.Dtos;

public class CreateRestaurantDiaryDto
{
    public Guid DiaryId { get; set; }
    public Guid RestaurantId { get; set; }
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
