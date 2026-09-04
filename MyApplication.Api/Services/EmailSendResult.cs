namespace MyApplication.Api.Services;

public class EmailSendResult
{
    public bool Sent { get; init; }
    public bool UsedDevelopmentFallback { get; init; }
    public string Message { get; init; } = string.Empty;
}
