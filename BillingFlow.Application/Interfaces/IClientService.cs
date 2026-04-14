using BillingFlow.Application.DTOs.Clients;
using BillingFlow.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Application.Interfaces
{
    public interface IClientService
    {
        Task<ClientResponseDto> CreateAsync(Guid userId, CreateClientRequestDto request);

        Task<PagedResultDto<ClientResponseDto>> GetAllAsync(Guid userId, ClientFilterRequestDto filter);
        Task<ClientResponseDto> UpdateAsync(Guid userId, Guid clientId, UpdateClientRequestDto request);
        Task DeleteAsync(Guid userId, Guid clientId);
    }
}
