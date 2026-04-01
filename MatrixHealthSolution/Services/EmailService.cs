using System.Net;
using System.Net.Mail;

namespace MatrixHealthSolution.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var host = _config["Email:SmtpHost"];
        var portStr = _config["Email:SmtpPort"];
        var username = _config["Email:Username"];
        var password = _config["Email:Password"];
        var from = _config["Email:From"];

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(portStr) ||
            string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(from))
        {
            throw new InvalidOperationException("Email is not configured.");
        }

        var smtp = new SmtpClient
        {
            Host = host,
            Port = int.Parse(portStr),
            EnableSsl = true,
            Credentials = new NetworkCredential(username, password)
        };

        var mail = new MailMessage
        {
            From = new MailAddress(from),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mail.To.Add(to);
        await smtp.SendMailAsync(mail);
    }
}
