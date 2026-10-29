using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Neptune.HubSpot.Webhook.Handling;
using Neptune.HubSpot.Webhook.Security;

namespace Neptune.HubSpot.Webhook.Controllers;

[ApiController]
[Route("webhooks/hubspot")]
public sealed class WebhooksController(
    HubSpotSignatureValidator validator,
    HubSpotEventProcessor processor,
    IOptions<HubSpotOptions> options) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Receive(CancellationToken cancellationToken)
    {
        // The signature covers the raw body, so read it before any model binding.
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);

        var requestUri = BuildPublicUri();
        if (!validator.IsValid(
                Request.Method, requestUri, body,
                Request.Headers[HubSpotSignatureValidator.SignatureHeader],
                Request.Headers[HubSpotSignatureValidator.TimestampHeader]))
        {
            return Unauthorized();
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(body);
        }
        catch (JsonException)
        {
            return BadRequest(new { error = new { code = "INVALID_JSON", message = "Body is not valid JSON." } });
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return BadRequest(new { error = new { code = "INVALID_PAYLOAD", message = "Body must be a JSON array of events." } });
            }

            var result = await processor.ProcessAsync(document.RootElement, cancellationToken);
            return Ok(result);
        }
    }

    private string BuildPublicUri()
    {
        var baseUrl = options.Value.PublicBaseUrl.TrimEnd('/');
        var pathAndQuery = Request.Path + Request.QueryString;
        return string.IsNullOrEmpty(baseUrl)
            ? $"{Request.Scheme}://{Request.Host}{pathAndQuery}"
            : baseUrl + pathAndQuery;
    }
}
