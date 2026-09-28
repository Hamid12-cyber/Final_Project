using System.Net;
using System.Net.Mail;

namespace MotoHM.Api.Shared.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var host = _config["Smtp:Host"];
        var portRaw = _config["Smtp:Port"];
        var username = _config["Smtp:Username"];
        var password = _config["Smtp:Password"];
        var fromEmail = _config["Smtp:FromEmail"] ?? username;
        var fromName = _config["Smtp:FromName"] ?? "MotoHM";

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            // Smtp konfiqurasiyası doldurulmayıbsa, email göndərmək əvəzinə loga yazırıq
            // ki, development zamanı token-i konsoldan görüb test edə biləsən.
            _logger.LogWarning("SMTP konfiqurasiyası tapılmadı. Email göndərilmədi. Subject: {Subject}, To: {ToEmail}, Body: {Body}",
                subject, toEmail, htmlBody);
            return;
        }

        var port = int.TryParse(portRaw, out var parsedPort) ? parsedPort : 587;

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail!, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);
        s
        await client.SendMailAsync(message, cancellationToken);
    }
}