namespace MyApplication.Api.Services;

public interface IEmailSender
{
    Task<EmailSendResult> SendAsync(string toEmail, string subject, string htmlBody, string textBody);
}
