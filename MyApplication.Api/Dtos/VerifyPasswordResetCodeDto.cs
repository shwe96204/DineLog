namespace MyApplication.Api.Dtos;

public class VerifyPasswordResetCodeDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
