using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using MyApplication.Api.Options;

namespace MyApplication.Api.Services;

public class SmtpEmailSender(
    IOptions<EmailOptions> options,
    IWebHostEnvironment environment,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task<EmailSendResult> SendAsync(string toEmail, string subject, string htmlBody, string textBody)
    {
        if (!IsConfigured(_options))
        {
            if (environment.IsDevelopment() && _options.UseDevelopmentLoggingFallback)
            {
                logger.LogWarning(
                    "Email configuration is missing. Development fallback email for {Email}. Subject: {Subject}. Body: {Body}",
                    toEmail,
                    subject,
                    string.IsNullOrWhiteSpace(textBody) ? htmlBody : textBody);
                return new EmailSendResult
                {
                    Sent = false,
                    UsedDevelopmentFallback = true,
                    Message = "Email settings are not configured, so the OTP was written to the API log instead of being sent."
                };
            }

            throw new InvalidOperationException("Email delivery is not configured.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(GetFromAddress(_options), _options.FromName),
            Subject = subject,
            Body = string.IsNullOrWhiteSpace(htmlBody) ? textBody : htmlBody,
            IsBodyHtml = !string.IsNullOrWhiteSpace(htmlBody)
        };

        message.To.Add(toEmail);

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);
        }

        await client.SendMailAsync(message);

        return new EmailSendResult
        {
            Sent = true,
            UsedDevelopmentFallback = false,
            Message = "Email sent successfully."
        };
    }

    private static bool IsConfigured(EmailOptions options) =>
        !string.IsNullOrWhiteSpace(options.Host)
        && options.Port > 0
        && !string.IsNullOrWhiteSpace(GetFromAddress(options))
        && !string.IsNullOrWhiteSpace(options.Username)
        && !string.IsNullOrWhiteSpace(options.Password);

    private static string GetFromAddress(EmailOptions options) =>
        string.IsNullOrWhiteSpace(options.FromAddress) ? options.Username : options.FromAddress;
}
