using BillingFlow.Application.DTOs.Clients;
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
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateClientRequestDto request)
        {
            var userId = GetUserId();

            var result = await _clientService.CreateAsync(userId, request);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ClientFilterRequestDto filter)
        {
            var userId = GetUserId();

            var result = await _clientService.GetAllAsync(userId, filter);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateClientRequestDto request)
        {
            var userId = GetUserId();

            var result = await _clientService.UpdateAsync(userId, id, request);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId();

            await _clientService.DeleteAsync(userId, id);

            return NoContent();
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.Parse(userIdClaim!);
        }
    }
}
