using MailKit.Net.Smtp;
using MimeKit;

namespace Email.API.Services;

public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var message = new MimeMessage();
            
            message.From.Add(new MailboxAddress("E-Shop System", "noreply@eshop.com"));
            
            message.To.Add(new MailboxAddress("", to));
            
            message.Subject = subject;
            
            message.Body = new TextPart("html") { Text = body };
            
            using var client = new SmtpClient();
            
            var smtpHost = configuration["EmailSettings:SmtpHost"] ?? "mailpit";
            var smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"] ?? "1025");
            var enableSsl = bool.Parse(configuration["EmailSettings:EnableSsl"] ?? "false");
            
            await client.ConnectAsync(smtpHost, smtpPort, enableSsl);
            
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            logger.LogInformation("📧 Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Failed to send email to {To}", to);
            throw;
        }
    }
}