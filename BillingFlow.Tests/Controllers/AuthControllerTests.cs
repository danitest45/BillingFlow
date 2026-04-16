using BillingFlow.Api.Controllers;
using BillingFlow.Application.DTOs.Auth;
using BillingFlow.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BillingFlow.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_ShouldReturnOkWithServiceResult()
    {
        var service = new Mock<IAuthService>();
        var expected = new AuthResponseDto { UserId = Guid.NewGuid(), Token = "token" };
        service.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequestDto>())).ReturnsAsync(expected);
        var controller = new AuthController(service.Object);

        var result = await controller.Register(new RegisterRequestDto());

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Login_ShouldReturnOkWithServiceResult()
    {
        var service = new Mock<IAuthService>();
        var expected = new AuthResponseDto { UserId = Guid.NewGuid(), Token = "token" };
        service.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>())).ReturnsAsync(expected);
        var controller = new AuthController(service.Object);

        var result = await controller.Login(new LoginRequestDto());

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }
}
