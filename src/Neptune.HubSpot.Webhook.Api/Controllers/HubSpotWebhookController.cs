using Microsoft.AspNetCore.Mvc;
using Neptune.HubSpot.Webhook.Api.Events;
using Neptune.HubSpot.Webhook.Api.Security;

namespace Neptune.HubSpot.Webhook.Api.Controllers;

[ApiController]
[Route("webhooks/hubspot")]
[ValidateHubSpotSignature]
public sealed class HubSpotWebhookController(WebhookEventQueue queue) : ControllerBase
{
    /// <summary>Receives a HubSpot batch (JSON array). Signature is verified by the resource filter.</summary>
    [HttpPost]
    public IActionResult Post([FromBody] List<WebhookEvent> events)
    {
        foreach (var webhookEvent in events)
        {
            if (!queue.TryEnqueue(webhookEvent))
            {
                // Queue full: ask HubSpot to retry the delivery.
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }

        return Ok();
    }
}
