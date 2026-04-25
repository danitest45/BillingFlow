using BillingFlow.Application.DTOs.Subscription;
using BillingFlow.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BillingFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupportController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public SupportController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> SendSupportMessage([FromBody] SupportRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Informe seu nome." });

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Informe seu e-mail." });

            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { message = "Informe sua mensagem." });

            await _emailService.SendSupportEmailAsync(
                request.Name,
                request.Email,
                request.Subject,
                request.Message);

            return Ok(new
            {
                message = "Mensagem enviada com sucesso. Responderemos assim que possível."
            });
        }
    }
}
