namespace Neptune.HubSpot.Webhook.Api.Events;

/// <summary>Drains the queue and dispatches events; a failing event is logged and does not stop the loop.</summary>
public sealed class WebhookDispatchService(
    WebhookEventQueue queue,
    WebhookEventDispatcher dispatcher,
    ILogger<WebhookDispatchService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var webhookEvent in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await dispatcher.DispatchAsync(webhookEvent, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Handling HubSpot event {EventId} failed", webhookEvent.EventId);
            }
        }
    }
}
