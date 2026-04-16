using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.Interfaces
{
    public interface IInvoiceAutomationService
    {
        Task<int> GenerateMissingInvoicesForCurrentMonthAsync(CancellationToken cancellationToken = default);
    }
}
