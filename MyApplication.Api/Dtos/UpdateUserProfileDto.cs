namespace MyApplication.Api.Dtos;

public class UpdateUserProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHint { get; set; } = string.Empty;
    public string ProfileImageDataUrl { get; set; } = string.Empty;
    public string ThemePreference { get; set; } = "light";
}
