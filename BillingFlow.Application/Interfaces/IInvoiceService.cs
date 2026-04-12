using BillingFlow.Application.DTOs.Invoices;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceResponseDto> GenerateAsync(Guid userId, Guid clientId);
        Task<List<InvoiceResponseDto>> GetAllAsync(Guid userId);
    }
}
