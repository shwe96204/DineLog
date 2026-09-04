namespace MyApplication.Api.Entities;

public class AppUser
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordHint { get; set; } = string.Empty;
    public DateTime DateJoined { get; set; }
    public string ProfileImageDataUrl { get; set; } = string.Empty;
    public string ThemePreference { get; set; } = "light";
    public bool IsEmailVerified { get; set; }
    public string EmailVerificationCodeHash { get; set; } = string.Empty;
    public DateTime? EmailVerificationExpiresAt { get; set; }
    public DateTime? EmailVerificationSentAt { get; set; }
    public string PasswordResetCodeHash { get; set; } = string.Empty;
    public DateTime? PasswordResetExpiresAt { get; set; }
    public DateTime? PasswordResetSentAt { get; set; }
    public DateTime? PasswordResetVerifiedAt { get; set; }

    public ICollection<Restaurant> Restaurants { get; set; } = [];
    public ICollection<RestaurantTag> Tags { get; set; } = [];
}
