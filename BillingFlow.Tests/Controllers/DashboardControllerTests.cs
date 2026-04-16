using BillingFlow.Api.Controllers;
using BillingFlow.Application.DTOs.Dashboard;
using BillingFlow.Application.Interfaces;
using BillingFlow.Tests.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BillingFlow.Tests.Controllers;

public class DashboardControllerTests
{
    [Fact]
    public async Task GetSummary_ShouldReturnOk()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<IDashboardService>();
        var expected = new DashboardSummaryDto();
        service.Setup(s => s.GetSummaryAsync(userId)).ReturnsAsync(expected);

        var controller = new DashboardController(service.Object);
        ControllerTestHelper.SetUser(controller, userId);

        var result = await controller.GetSummary();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }
}
