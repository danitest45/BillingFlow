using BillingFlow.Api.Controllers;
using BillingFlow.Application.DTOs.Clients;
using BillingFlow.Application.DTOs.Common;
using BillingFlow.Application.Interfaces;
using BillingFlow.Tests.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BillingFlow.Tests.Controllers;

public class ClientsControllerTests
{
    [Fact]
    public async Task Create_ShouldReturnOk()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<IClientService>();
        var expected = new ClientResponseDto { Id = Guid.NewGuid(), Name = "A" };

        service.Setup(s => s.CreateAsync(userId, It.IsAny<CreateClientRequestDto>())).ReturnsAsync(expected);

        var controller = new ClientsController(service.Object);
        ControllerTestHelper.SetUser(controller, userId);

        var result = await controller.Create(new CreateClientRequestDto());

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<IClientService>();
        var expected = new PagedResultDto<ClientResponseDto>();

        service.Setup(s => s.GetAllAsync(userId, It.IsAny<ClientFilterRequestDto>())).ReturnsAsync(expected);

        var controller = new ClientsController(service.Object);
        ControllerTestHelper.SetUser(controller, userId);

        var result = await controller.GetAll(new ClientFilterRequestDto());

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Update_ShouldReturnOk()
    {
        var userId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var service = new Mock<IClientService>();
        var expected = new ClientResponseDto { Id = clientId };

        service.Setup(s => s.UpdateAsync(userId, clientId, It.IsAny<UpdateClientRequestDto>())).ReturnsAsync(expected);

        var controller = new ClientsController(service.Object);
        ControllerTestHelper.SetUser(controller, userId);

        var result = await controller.Update(clientId, new UpdateClientRequestDto());

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent()
    {
        var userId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var service = new Mock<IClientService>();

        service.Setup(s => s.DeleteAsync(userId, clientId)).Returns(Task.CompletedTask);

        var controller = new ClientsController(service.Object);
        ControllerTestHelper.SetUser(controller, userId);

        var result = await controller.Delete(clientId);

        Assert.IsType<NoContentResult>(result);
    }
}
