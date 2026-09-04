namespace MyApplication.Api.Options;

public class EmailOptions
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "MyApplication";
    public bool EnableSsl { get; set; } = true;
    public bool UseDevelopmentLoggingFallback { get; set; } = true;
}
