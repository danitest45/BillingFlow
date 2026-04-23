using BillingFlow.Application.DTOs.Auth;
using BillingFlow.Application.Interfaces;
using BillingFlow.Domain.Entities;
using BillingFlow.Domain.Enums;
using BillingFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BillingFlow.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly BillingFlowDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(
            BillingFlowDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (existingUser is not null)
                throw new Exception("E-mail já cadastrado.");

            var now = DateTime.UtcNow;

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = now
            };

            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                PlanType = PlanType.Trial,
                Status = SubscriptionStatus.Trialing,
                MaxClients = 10,
                StartsAt = now,
                EndsAt = now.AddDays(7),
                CreatedAt = now
            };

            _context.Users.Add(user);
            _context.Subscriptions.Add(subscription);

            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Token = GenerateJwtToken(user)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user is null)
                throw new Exception("Usuário não encontrado.");

            var validPassword = BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);

            if (!validPassword)
                throw new Exception("Senha inválida.");

            return new AuthResponseDto
            {
                UserId = user.Id,
                Token = GenerateJwtToken(user)
            };
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["Jwt:ExpirationInMinutes"]!)),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
