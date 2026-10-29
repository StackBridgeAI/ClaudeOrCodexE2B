namespace Neptune.HubSpot.Webhook.Api.Events;

/// <summary>Default handler: logs identifiers only, never property values.</summary>
public sealed partial class LoggingWebhookEventHandler(
    WebhookObjectType objectType,
    ILogger<LoggingWebhookEventHandler> logger) : IWebhookEventHandler
{
    public WebhookObjectType ObjectType { get; } = objectType;

    public Task HandleAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken)
    {
        LogEvent(logger, webhookEvent.EventId, webhookEvent.SubscriptionType, ObjectType,
            webhookEvent.ObjectId, webhookEvent.OccurredAt);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "HubSpot event {EventId} {SubscriptionType} ({ObjectType} {ObjectId}) occurred at {OccurredAt}")]
    private static partial void LogEvent(ILogger logger, long eventId, string subscriptionType,
        WebhookObjectType objectType, long objectId, long occurredAt);
}
