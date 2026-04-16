using BillingFlow.Api.Controllers;
using BillingFlow.Application.DTOs.Common;
using BillingFlow.Application.DTOs.Invoices;
using BillingFlow.Application.Interfaces;
using BillingFlow.Domain.Enums;
using BillingFlow.Tests.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BillingFlow.Tests.Controllers;

public class InvoicesControllerTests
{
    [Fact]
    public async Task Generate_ShouldReturnOk()
    {
        var userId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var service = new Mock<IInvoiceService>();
        var expected = new InvoiceResponseDto { Id = Guid.NewGuid(), ClientId = clientId };
        service.Setup(s => s.GenerateAsync(userId, clientId)).ReturnsAsync(expected);

        var controller = new InvoicesController(service.Object);
        ControllerTestHelper.SetUser(controller, userId);

        var result = await controller.Generate(clientId);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<IInvoiceService>();
        var expected = new PagedResultDto<InvoiceResponseDto>();
        service.Setup(s => s.GetAllAsync(userId, It.IsAny<InvoiceFilterRequestDto>())).ReturnsAsync(expected);

        var controller = new InvoicesController(service.Object);
        ControllerTestHelper.SetUser(controller, userId);

        var result = await controller.GetAll(new InvoiceFilterRequestDto());

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task MarkAsPaid_ShouldReturnNotFound_WhenServiceReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var service = new Mock<IInvoiceService>();
        service.Setup(s => s.MarkAsPaidAsync(userId, invoiceId)).ReturnsAsync(false);

        var controller = new InvoicesController(service.Object);
        ControllerTestHelper.SetUser(controller, userId);

        var result = await controller.MarkAsPaid(invoiceId);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Cobrança não encontrada.", notFound.Value);
    }

    [Fact]
    public async Task MarkAsPaid_ShouldReturnOk_WhenServiceReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var service = new Mock<IInvoiceService>();
        service.Setup(s => s.MarkAsPaidAsync(userId, invoiceId)).ReturnsAsync(true);

        var controller = new InvoicesController(service.Object);
        ControllerTestHelper.SetUser(controller, userId);

        var result = await controller.MarkAsPaid(invoiceId);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Cobrança marcada como paga.", ok.Value);
    }
}
