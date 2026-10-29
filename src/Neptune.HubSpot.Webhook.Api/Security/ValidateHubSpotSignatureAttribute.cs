using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Neptune.HubSpot.Webhook.Api.Security;

/// <summary>Resource filter: rejects with 401 before model binding when the HubSpot signature is not valid.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ValidateHubSpotSignatureAttribute : Attribute, IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var request = context.HttpContext.Request;
        request.EnableBuffering();

        string body;
        using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync(context.HttpContext.RequestAborted);
        }
        request.Body.Position = 0;

        var validator = context.HttpContext.RequestServices.GetRequiredService<HubSpotSignatureValidator>();
        var valid = validator.IsValid(
            request.Method,
            request.Path.ToUriComponent() + request.QueryString.ToUriComponent(),
            body,
            request.Headers[HubSpotSignatureValidator.SignatureHeader],
            request.Headers[HubSpotSignatureValidator.TimestampHeader]);

        if (!valid)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        await next();
    }
}
