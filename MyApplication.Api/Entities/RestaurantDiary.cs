namespace MyApplication.Api.Entities;

public class RestaurantDiary
{
    public Guid Id { get; set; }
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
    public string TagIdsJson { get; set; } = "[]";
    public string GalleryImagesJson { get; set; } = "[]";

    public DiaryFolder? Diary { get; set; }
    public Restaurant? Restaurant { get; set; }
}
