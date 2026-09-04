namespace Shared.Models;

public class DiaryFolderItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DiaryName { get; set; } = string.Empty;
    public string Color { get; set; } = "#d19044";
    public DateTime DateCreated { get; set; }
    public string CoverImageDataUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RestaurantDiaryCount { get; set; }
}
