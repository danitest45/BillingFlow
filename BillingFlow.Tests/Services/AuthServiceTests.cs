using BillingFlow.Application.DTOs.Auth;
using BillingFlow.Domain.Entities;
using BillingFlow.Infrastructure.Services;
using BillingFlow.Tests.Common;
using Microsoft.Extensions.Configuration;

namespace BillingFlow.Tests.Services;

public class AuthServiceTests
{
    private static IConfiguration CreateConfiguration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "THIS-IS-A-VERY-LONG-KEY-FOR-TESTS-ONLY-1234567890",
            ["Jwt:Issuer"] = "BillingFlow.Tests",
            ["Jwt:Audience"] = "BillingFlow.Tests.Audience",
            ["Jwt:ExpirationInMinutes"] = "60"
        })
        .Build();

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserAndReturnToken()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new AuthService(context, CreateConfiguration());

        var response = await service.RegisterAsync(new RegisterRequestDto
        {
            Name = "Ana",
            Email = "ana@email.com",
            Password = "123456"
        });

        var user = await context.Users.FindAsync(response.UserId);
        Assert.NotNull(user);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.NotEqual("123456", user!.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        using var context = TestDbContextFactory.CreateContext();
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Existing",
            Email = "existing@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123")
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, CreateConfiguration());

        var ex = await Assert.ThrowsAsync<Exception>(() => service.RegisterAsync(new RegisterRequestDto
        {
            Name = "Ana",
            Email = "existing@email.com",
            Password = "123456"
        }));

        Assert.Equal("E-mail já cadastrado.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        using var context = TestDbContextFactory.CreateContext();
        var service = new AuthService(context, CreateConfiguration());

        var ex = await Assert.ThrowsAsync<Exception>(() => service.LoginAsync(new LoginRequestDto
        {
            Email = "none@email.com",
            Password = "123456"
        }));

        Assert.Equal("Usuário não encontrado.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIsInvalid()
    {
        using var context = TestDbContextFactory.CreateContext();
        context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Name = "Ana",
            Email = "ana@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("right")
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, CreateConfiguration());

        var ex = await Assert.ThrowsAsync<Exception>(() => service.LoginAsync(new LoginRequestDto
        {
            Email = "ana@email.com",
            Password = "wrong"
        }));

        Assert.Equal("Senha inválida.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        using var context = TestDbContextFactory.CreateContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Ana",
            Email = "ana@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456")
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, CreateConfiguration());

        var response = await service.LoginAsync(new LoginRequestDto
        {
            Email = "ana@email.com",
            Password = "123456"
        });

        Assert.Equal(user.Id, response.UserId);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
    }
}
