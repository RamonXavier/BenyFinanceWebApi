using System.Net;
using System.Net.Mail;
using BenyFinance.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BenyFinance.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken)
    {
        var smtpHost = _configuration["Smtp:Host"] ?? throw new ArgumentException("SMTP Host is not configured");
        var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
        var smtpEmail = _configuration["Smtp:Email"] ?? throw new ArgumentException("SMTP Email is not configured");
        var smtpPassword = _configuration["Smtp:Password"] ?? throw new ArgumentException("SMTP Password is not configured");
        var fromName = _configuration["Smtp:FromName"] ?? "BenyFinance";
        var frontendUrl = _configuration["App:FrontendUrl"] ?? "https://benyfinance.netlify.app";

        var resetUrl = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(toEmail)}";

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpEmail, fromName),
            Subject = "Recuperação de Senha - BenyFinance",
            IsBodyHtml = true,
            Body = $@"
<!DOCTYPE html>
<html lang=""pt-BR"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Recuperação de Senha</title>
    <style>
        body {{
            font-family: 'Segoe UI', Arial, sans-serif;
            line-height: 1.6;
            margin: 0;
            padding: 0;
            background-color: #f3f4f6;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            text-align: center;
            padding: 20px 0;
            background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);
            border-radius: 8px 8px 0 0;
            margin: -20px -20px 20px -20px;
        }}
        .header h1 {{
            color: #ffffff;
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            padding: 0 20px 20px 20px;
        }}
        .greeting {{
            font-size: 18px;
            color: #374151;
            margin-bottom: 20px;
        }}
        .message {{
            color: #4b5563;
            margin-bottom: 30px;
        }}
        .button-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 14px 40px;
            background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);
            color: #ffffff;
            text-decoration: none;
            border-radius: 8px;
            font-weight: 600;
            font-size: 16px;
            box-shadow: 0 4px 6px rgba(37, 99, 235, 0.3);
        }}
        .button:hover {{
            background: linear-gradient(135deg, #1d4ed8 0%, #1e40af 100%);
        }}
        .footer {{
            margin-top: 40px;
            padding-top: 20px;
            border-top: 1px solid #e5e7eb;
            text-align: center;
            color: #9ca3af;
            font-size: 14px;
        }}
        .warning {{
            background-color: #fef3c7;
            border-left: 4px solid #f59e0b;
            padding: 12px 16px;
            margin: 20px 0;
            border-radius: 4px;
            color: #78350f;
            font-size: 14px;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🔐 Recuperação de Senha</h1>
        </div>
        <div class=""content"">
            <p class=""greeting"">Olá, {toName}!</p>
            <p class=""message"">
                Recebemos uma solicitação para redefinir a senha da sua conta no BenyFinance. 
                Clique no botão abaixo para criar uma nova senha:
            </p>
            <div class=""button-container"">
                <a href=""{resetUrl}"" class=""button"">Redefinir Senha</a>
            </div>
            <div class=""warning"">
                ⚠️ Este link expira em 1 hora. Se você não solicitou a recuperação de senha, 
                pode ignorar este email com segurança.
            </div>
            <div class=""footer"">
                <p>Atenciosamente,<br>Equipe BenyFinance</p>
                <p style=""margin-top: 10px; font-size: 12px;"">
                    Este é um email automático, por favor não responda.
                </p>
            </div>
        </div>
    </div>
</body>
</html>"
        };
        mailMessage.To.Add(toEmail);

        using var smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpEmail, smtpPassword),
            EnableSsl = true
        };

        await smtpClient.SendMailAsync(mailMessage);
    }
}