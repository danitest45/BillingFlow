using BillingFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AutomationController : ControllerBase
    {
        private readonly IInvoiceAutomationService _invoiceAutomationService;

        public AutomationController(IInvoiceAutomationService invoiceAutomationService)
        {
            _invoiceAutomationService = invoiceAutomationService;
        }

        [HttpPost("generate-current-month")]
        public async Task<IActionResult> GenerateCurrentMonth(CancellationToken cancellationToken)
        {
            var created = await _invoiceAutomationService
                .GenerateMissingInvoicesForCurrentMonthAsync(cancellationToken);

            return Ok(new
            {
                created
            });
        }
    }
}
