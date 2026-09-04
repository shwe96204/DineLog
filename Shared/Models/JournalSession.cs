namespace Shared.Models;

public class JournalSession
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateJoined { get; set; }
    public DateTime LoggedInAt { get; set; }
    public string ThemePreference { get; set; } = "light";
    public bool IsEmailVerified { get; set; }
}
