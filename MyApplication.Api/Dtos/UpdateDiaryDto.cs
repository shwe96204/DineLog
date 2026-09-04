namespace MyApplication.Api.Dtos;

public class UpdateDiaryDto
{
    public Guid UserId { get; set; }
    public string DiaryName { get; set; } = string.Empty;
    public string Color { get; set; } = "#d19044";
    public string CoverImageDataUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
