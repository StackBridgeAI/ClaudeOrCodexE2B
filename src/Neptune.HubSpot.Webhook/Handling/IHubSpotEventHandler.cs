using Neptune.HubSpot.Webhook.Models;

namespace Neptune.HubSpot.Webhook.Handling;

/// <summary>Extension point for business handling of a supported HubSpot event.</summary>
public interface IHubSpotEventHandler
{
    Task HandleAsync(HubSpotEvent hubSpotEvent, CancellationToken cancellationToken);
}
