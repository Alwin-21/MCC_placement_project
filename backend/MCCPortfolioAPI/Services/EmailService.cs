using System;
using System.IO;
using System.Threading.Tasks;

namespace MCCPortfolioAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "sent-emails.txt");
                var emailContent = $"\n========================================\n" +
                                   $"DATE: {DateTime.UtcNow}\n" +
                                   $"TO: {toEmail}\n" +
                                   $"SUBJECT: {subject}\n" +
                                   $"BODY:\n{body}\n" +
                                   $"========================================\n";
                
                await File.AppendAllTextAsync(logPath, emailContent);
                System.Console.WriteLine($"[EMAIL SENT] To: {toEmail}, Subject: {subject}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[EMAIL ERROR] Failed to send email to {toEmail}: {ex.Message}");
            }
        }
    }
}
