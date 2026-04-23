using BillingFlow.Application.DTOs.MessageTemplate;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.Interfaces
{
    public interface IMessageTemplateService
    {
        Task<MessageTemplateResponseDto> GetAsync(Guid userId);
        Task<MessageTemplateResponseDto> UpdateAsync(Guid userId, UpdateMessageTemplateRequestDto request);
        Task<WhatsAppPreviewResponseDto> GenerateWhatsAppPreviewAsync(Guid userId, Guid invoiceId);
    }
}
