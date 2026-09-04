namespace MyApplication.Api.Dtos;

public class UserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHint { get; set; } = string.Empty;
    public DateTime DateJoined { get; set; }
    public string ProfileImageDataUrl { get; set; } = string.Empty;
    public string ThemePreference { get; set; } = "light";
    public bool IsEmailVerified { get; set; }
}
