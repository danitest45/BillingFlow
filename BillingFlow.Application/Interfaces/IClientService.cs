using BillingFlow.Application.DTOs.Clients;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.Interfaces
{
    public interface IClientService
    {
        Task<ClientResponseDto> CreateAsync(Guid userId, CreateClientRequestDto request);

        Task<List<ClientResponseDto>> GetAllAsync(Guid userId);
        Task<ClientResponseDto> UpdateAsync(Guid userId, Guid clientId, UpdateClientRequestDto request);
        Task DeleteAsync(Guid userId, Guid clientId);
    }
}
