using Neptune.HubSpot.Webhook.Models;

namespace Neptune.HubSpot.Webhook.Handling;

/// <summary>Default handler: writes a structured log entry. Property values are not logged (may be PII).</summary>
public sealed partial class LoggingHubSpotEventHandler(ILogger<LoggingHubSpotEventHandler> logger) : IHubSpotEventHandler
{
    public Task HandleAsync(HubSpotEvent hubSpotEvent, CancellationToken cancellationToken)
    {
        LogEvent(hubSpotEvent.EventId, hubSpotEvent.SubscriptionType, hubSpotEvent.ObjectId,
            hubSpotEvent.PropertyName, hubSpotEvent.PortalId);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "HubSpot event {EventId} {SubscriptionType} object {ObjectId} property {PropertyName} portal {PortalId}")]
    private partial void LogEvent(long eventId, string? subscriptionType, long objectId, string? propertyName, long portalId);
}
