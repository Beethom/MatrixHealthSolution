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
        var smtp = new SmtpClient
        {
            Host = _config["Email:SmtpHost"],
            Port = int.Parse(_config["Email:SmtpPort"]!),
            EnableSsl = true,
            Credentials = new NetworkCredential(
                _config["Email:Username"],
                _config["Email:Password"]
            )
        };

        var mail = new MailMessage
        {
            From = new MailAddress(_config["Email:From"]!),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mail.To.Add(to);
        await smtp.SendMailAsync(mail);
    }
}
