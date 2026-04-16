using BillingFlow.Api.Controllers;
using BillingFlow.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BillingFlow.Tests.Controllers;

public class AutomationControllerTests
{
    [Fact]
    public async Task GenerateCurrentMonth_ShouldReturnOkWithCreatedCount()
    {
        var service = new Mock<IInvoiceAutomationService>();
        service.Setup(s => s.GenerateMissingInvoicesForCurrentMonthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var controller = new AutomationController(service.Object);

        var result = await controller.GenerateCurrentMonth(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);

        var createdProp = ok.Value!.GetType().GetProperty("created");
        Assert.NotNull(createdProp);
        Assert.Equal(3, createdProp!.GetValue(ok.Value));
    }
}
