namespace Neptune.HubSpot.Webhook.Api.Events;

/// <summary>Seam for business processing of one object type; replace the default to add behaviour.</summary>
public interface IWebhookEventHandler
{
    WebhookObjectType ObjectType { get; }

    Task HandleAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken);
}
