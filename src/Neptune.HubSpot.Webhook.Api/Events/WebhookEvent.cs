using System.Text.Json.Serialization;

namespace Neptune.HubSpot.Webhook.Api.Events;

/// <summary>One event of a HubSpot webhook batch. Unknown JSON fields are ignored.</summary>
public sealed record WebhookEvent
{
    [JsonPropertyName("eventId")] public long EventId { get; init; }
    [JsonPropertyName("subscriptionType")] public string SubscriptionType { get; init; } = string.Empty;
    [JsonPropertyName("objectId")] public long ObjectId { get; init; }
    [JsonPropertyName("occurredAt")] public long OccurredAt { get; init; }
}
