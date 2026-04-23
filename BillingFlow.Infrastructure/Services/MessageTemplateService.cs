using BillingFlow.Application.DTOs.MessageTemplate;
using BillingFlow.Application.Interfaces;
using BillingFlow.Domain.Helper;
using BillingFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Infrastructure.Services
{
    public class MessageTemplateService : IMessageTemplateService
    {
        private readonly BillingFlowDbContext _context;

        private const string DefaultTemplate =
@"Olá [nome], tudo bem?

Estou entrando em contato sobre a cobrança de R$[valor], com vencimento em [vencimento].

Poderia verificar pra mim?";

        public MessageTemplateService(BillingFlowDbContext context)
        {
            _context = context;
        }

        public async Task<MessageTemplateResponseDto> GetAsync(Guid userId)
        {
            var subscription = await _context.Subscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            SubscriptionFeatureHelper.EnsureCanUseWhatsApp(subscription);
            
            var template = await _context.MessageTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (template == null)
            {
                return new MessageTemplateResponseDto
                {
                    ChargeTemplate = DefaultTemplate
                };
            }

            return new MessageTemplateResponseDto
            {
                ChargeTemplate = template.ChargeTemplate
            };
        }

        public async Task<MessageTemplateResponseDto> UpdateAsync(Guid userId, UpdateMessageTemplateRequestDto request)
        {
            var subscription = await _context.Subscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            SubscriptionFeatureHelper.EnsureCanUseWhatsApp(subscription);

            var template = await _context.MessageTemplates
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (template == null)
            {
                template = new Domain.Entities.MessageTemplate
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ChargeTemplate = request.ChargeTemplate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.MessageTemplates.Add(template);
            }
            else
            {
                template.ChargeTemplate = request.ChargeTemplate;
                template.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new MessageTemplateResponseDto
            {
                ChargeTemplate = template.ChargeTemplate
            };
        }

        public async Task<WhatsAppPreviewResponseDto> GenerateWhatsAppPreviewAsync(Guid userId, Guid invoiceId)
        {
            var subscription = await _context.Subscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            SubscriptionFeatureHelper.EnsureCanUseWhatsApp(subscription);

            var invoice = await _context.InvoiceRecords
                .Include(i => i.Client)
                .FirstOrDefaultAsync(i => i.Id == invoiceId && i.Client.UserId == userId);

            if (invoice == null)
                throw new Exception("Cobrança não encontrada.");

            var template = await _context.MessageTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            var baseTemplate = template?.ChargeTemplate ?? DefaultTemplate;

            var message = baseTemplate
                .Replace("[nome]", invoice.Client.Name)
                .Replace("[valor]", invoice.Amount.ToString("N2"))
                .Replace("[vencimento]", invoice.DueDate.ToString("dd/MM/yyyy"));

            var phone = NormalizeBrazilPhone(invoice.Client.Phone);

            var encodedMessage = Uri.EscapeDataString(message);
            var whatsAppUrl = $"https://wa.me/{phone}?text={encodedMessage}";

            return new WhatsAppPreviewResponseDto
            {
                Phone = phone,
                Message = message,
                WhatsAppUrl = whatsAppUrl
            };
        }

        private static string NormalizeBrazilPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            var digits = new string(phone.Where(char.IsDigit).ToArray());

            if (digits.StartsWith("55"))
                return digits;

            return $"55{digits}";
        }
    }
}
