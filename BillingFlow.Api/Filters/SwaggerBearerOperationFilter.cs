using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BillingFlow.Api.Filters
{
    public class SwaggerBearerOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAuthorize =
                context.MethodInfo.GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .Any()
                ||
                context.MethodInfo.DeclaringType!
                    .GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
                    .Any();

            if (!hasAuthorize)
                return;

            operation.Security ??= new List<OpenApiSecurityRequirement>();

            var securityScheme = new OpenApiSecuritySchemeReference("Bearer");

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                { securityScheme, new List<string>() }
            });
        }
    }
}
