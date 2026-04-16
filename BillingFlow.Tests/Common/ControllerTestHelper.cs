using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BillingFlow.Tests.Common;

internal static class ControllerTestHelper
{
    public static void SetUser(ControllerBase controller, Guid userId)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                ],
                "TestAuth"))
            }
        };
    }
}
