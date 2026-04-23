using BillingFlow.Application.DTOs.MessageTemplate;
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
    public class MessageTemplateController : ControllerBase
    {
        private readonly IMessageTemplateService _messageTemplateService;

        public MessageTemplateController(IMessageTemplateService messageTemplateService)
        {
            _messageTemplateService = messageTemplateService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = GetUserId();
            var result = await _messageTemplateService.GetAsync(userId);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateMessageTemplateRequestDto request)
        {
            var userId = GetUserId();
            var result = await _messageTemplateService.UpdateAsync(userId, request);
            return Ok(result);
        }

        [HttpGet("whatsapp-preview/{invoiceId}")]
        public async Task<IActionResult> GetWhatsAppPreview(Guid invoiceId)
        {
            var userId = GetUserId();
            var result = await _messageTemplateService.GenerateWhatsAppPreviewAsync(userId, invoiceId);
            return Ok(result);
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }
    }
}
