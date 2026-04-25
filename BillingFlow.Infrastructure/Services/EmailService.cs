using BillingFlow.Application.Interfaces;
using BillingFlow.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using Resend;

namespace BillingFlow.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly EmailSettings _emailSettings;

        public EmailService(
            IResend resend,
            IOptions<EmailSettings> emailOptions)
        {
            _resend = resend;
            _emailSettings = emailOptions.Value;
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetLink)
        {
            var message = new EmailMessage
            {
                From = _emailSettings.From,
                Subject = "Redefinição de senha - CobrançaFlow",
                HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #111827;'>
                        <h2>Redefinição de senha</h2>

                        <p>Recebemos uma solicitação para redefinir sua senha.</p>

                        <p>Clique no botão abaixo para criar uma nova senha:</p>

                        <p>
                            <a href='{resetLink}'
                               style='display: inline-block; padding: 12px 18px; background: #111827; color: white; text-decoration: none; border-radius: 8px;'>
                               Redefinir senha
                            </a>
                        </p>

                        <p>Este link expira em 30 minutos.</p>

                        <p>Se você não solicitou isso, ignore este e-mail.</p>
                    </div>"
            };

            message.To.Add(to);

            await _resend.EmailSendAsync(message);
        }

        public async Task SendSupportEmailAsync(string name, string email, string subject, string messageText)
        {
            var safeSubject = string.IsNullOrWhiteSpace(subject)
                ? "Nova mensagem de suporte"
                : subject;

            var message = new EmailMessage
            {
                From = _emailSettings.From,
                Subject = $"Suporte CobrançaFlow - {safeSubject}",
                HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #111827;'>
                <h2>Nova mensagem de suporte</h2>

                <p><strong>Nome:</strong> {name}</p>
                <p><strong>E-mail:</strong> {email}</p>
                <p><strong>Assunto:</strong> {safeSubject}</p>

                <hr />

                <p><strong>Mensagem:</strong></p>
                <p>{messageText}</p>
            </div>"
            };

            message.To.Add(_emailSettings.SupportTo);

            message.ReplyTo = email;

            await _resend.EmailSendAsync(message);
        }
    }
}
