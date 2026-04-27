using BillingFlow.Application.DTOs.Invoices;
using BillingFlow.Application.Interfaces;
using BillingFlow.Domain.Entities;
using BillingFlow.Domain.Exceptions;
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

            try
            {
                var result = await _invoiceService.GenerateAsync(userId, clientId);
                return Ok(result);
            }
            catch (InvoiceAlreadyExistsException ex)
            {
                return Conflict(new
                {
                    code = "INVOICE_ALREADY_EXISTS",
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
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

        [HttpPost("replace/{clientId}")]
        public async Task<IActionResult> Replace(Guid clientId)
        {
            var userId = GetUserId();

            try
            {
                var result = await _invoiceService.ReplaceAsync(userId, clientId);
                return Ok(result);
            }
            catch (InvoiceAlreadyPaidException ex)
            {
                return BadRequest(new
                {
                    code = "INVOICE_ALREADY_PAID",
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = GetUserId();

                var success = await _invoiceService.DeleteAsync(userId, id);

                if (!success)
                {
                    return NotFound(new
                    {
                        message = "Cobrança não encontrada."
                    });
                }

                return Ok(new
                {
                    message = "Cobrança excluída com sucesso."
                });
            }
            catch (InvoiceAlreadyPaidException ex)
            {
                return BadRequest(new
                {
                    code = "INVOICE_ALREADY_PAID",
                    message = ex.Message
                });
            }
        }
    }
}
