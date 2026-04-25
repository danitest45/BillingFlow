using BillingFlow.Application.DTOs.Auth;
using BillingFlow.Application.Interfaces;
using BillingFlow.Domain.Entities;
using BillingFlow.Domain.Enums;
using BillingFlow.Infrastructure.Persistence;
using BillingFlow.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;

        public AuthService(
            BillingFlowDbContext context,
            IConfiguration configuration,
            IEmailService emailService,
            IOptions<EmailSettings> emailOptions)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _emailSettings = emailOptions.Value;
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

        public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            var email = request.Email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

            // Segurança: não revela se o e-mail existe ou não.
            if (user == null)
                return;

            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "");

            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);

            await _context.SaveChangesAsync();

            var resetLink = $"{_emailSettings.FrontendUrl}/reset-password?token={Uri.EscapeDataString(token)}";

            await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                throw new Exception("Token inválido.");

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                throw new Exception("A senha deve ter pelo menos 6 caracteres.");

            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.PasswordResetToken == request.Token &&
                    x.PasswordResetTokenExpiresAt != null &&
                    x.PasswordResetTokenExpiresAt > DateTime.UtcNow);

            if (user == null)
                throw new Exception("Link de redefinição inválido ou expirado.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiresAt = null;

            await _context.SaveChangesAsync();
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
