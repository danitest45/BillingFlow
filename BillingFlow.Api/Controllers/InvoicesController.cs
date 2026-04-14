using BillingFlow.Application.DTOs.Invoices;
using BillingFlow.Application.Interfaces;
using BillingFlow.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BillingFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpPost("generate/{clientId}")]
        public async Task<IActionResult> Generate(Guid clientId)
        {
            var userId = GetUserId();

            var result = await _invoiceService.GenerateAsync(userId, clientId);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] InvoiceFilterRequestDto filter)
        {
            var userId = GetUserId();

            var result = await _invoiceService.GetAllAsync(userId, filter);

            return Ok(result);
        }

        [HttpPatch("{id}/pay")]
        public async Task<IActionResult> MarkAsPaid(Guid id)
        {
            var userId = GetUserId();

            var success = await _invoiceService.MarkAsPaidAsync(userId, id);

            if (!success)
                return NotFound("Cobrança não encontrada.");

            return Ok("Cobrança marcada como paga.");
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.Parse(userIdClaim!);
        }
    }
}
