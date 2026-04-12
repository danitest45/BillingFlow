using BillingFlow.Application.DTOs.Clients;
using BillingFlow.Application.Interfaces;
using BillingFlow.Domain.Entities;
using BillingFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillingFlow.Infrastructure.Services
{
    public class ClientService : IClientService
    {
        private readonly BillingFlowDbContext _context;

        public ClientService(BillingFlowDbContext context)
        {
            _context = context;
        }

        public async Task<ClientResponseDto> CreateAsync(
            Guid userId,
            CreateClientRequestDto request)
        {
            var client = new Client
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                MonthlyAmount = request.MonthlyAmount,
                DueDay = request.DueDay,
                IsActive = true
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return new ClientResponseDto
            {
                Id = client.Id,
                Name = client.Name,
                Email = client.Email,
                Phone = client.Phone,
                MonthlyAmount = client.MonthlyAmount,
                DueDay = client.DueDay
            };
        }

        public async Task<List<ClientResponseDto>> GetAllAsync(Guid userId)
        {
            return await _context.Clients
                .Where(x => x.UserId == userId)
                .Select(x => new ClientResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email,
                    Phone = x.Phone,
                    MonthlyAmount = x.MonthlyAmount,
                    DueDay = x.DueDay
                })
                .ToListAsync();
        }

        public async Task<ClientResponseDto> UpdateAsync(
            Guid userId,
            Guid clientId,
            UpdateClientRequestDto request)
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(x =>
                    x.Id == clientId &&
                    x.UserId == userId);

            if (client is null)
                throw new Exception("Cliente não encontrado.");

            client.Name = request.Name;
            client.Email = request.Email;
            client.Phone = request.Phone;
            client.MonthlyAmount = request.MonthlyAmount;
            client.DueDay = request.DueDay;

            await _context.SaveChangesAsync();

            return new ClientResponseDto
            {
                Id = client.Id,
                Name = client.Name,
                Email = client.Email,
                Phone = client.Phone,
                MonthlyAmount = client.MonthlyAmount,
                DueDay = client.DueDay
            };
        }

        public async Task DeleteAsync(Guid userId, Guid clientId)
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(x =>
                    x.Id == clientId &&
                    x.UserId == userId);

            if (client is null)
                throw new Exception("Cliente não encontrado.");

            _context.Clients.Remove(client);

            await _context.SaveChangesAsync();
        }
    }
}
