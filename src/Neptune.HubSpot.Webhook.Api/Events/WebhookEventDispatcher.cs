namespace Neptune.HubSpot.Webhook.Api.Events;

/// <summary>Routes an event to the handler of its object type. Event ids are logged so duplicates are visible.</summary>
public sealed partial class WebhookEventDispatcher(
    IEnumerable<IWebhookEventHandler> handlers,
    ILogger<WebhookEventDispatcher> logger)
{
    private readonly Dictionary<WebhookObjectType, IWebhookEventHandler> _handlers =
        handlers.ToDictionary(h => h.ObjectType);

    public async Task DispatchAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken)
    {
        var type = WebhookObjectTypeParser.Parse(webhookEvent.SubscriptionType);
        if (type == WebhookObjectType.Unknown || !_handlers.TryGetValue(type, out var handler))
        {
            LogIgnored(logger, webhookEvent.EventId, webhookEvent.SubscriptionType);
            return;
        }

        LogDispatching(logger, webhookEvent.EventId, type);
        await handler.HandleAsync(webhookEvent, cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Ignored HubSpot event {EventId} of type '{SubscriptionType}'")]
    private static partial void LogIgnored(ILogger logger, long eventId, string subscriptionType);

    [LoggerMessage(Level = LogLevel.Information, Message = "Dispatching HubSpot event {EventId} to {ObjectType} handler")]
    private static partial void LogDispatching(ILogger logger, long eventId, WebhookObjectType objectType);
}
