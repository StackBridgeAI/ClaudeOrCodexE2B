using System.Text.Json.Serialization;

namespace Neptune.HubSpot.Webhook.Models;

/// <summary>One HubSpot webhook event (the POST body is a JSON array of these).</summary>
public sealed record HubSpotEvent
{
    [JsonPropertyName("eventId")] public long EventId { get; init; }
    [JsonPropertyName("subscriptionId")] public long SubscriptionId { get; init; }
    [JsonPropertyName("portalId")] public long PortalId { get; init; }
    [JsonPropertyName("occurredAt")] public long OccurredAt { get; init; }
    [JsonPropertyName("subscriptionType")] public string? SubscriptionType { get; init; }
    [JsonPropertyName("objectId")] public long ObjectId { get; init; }
    [JsonPropertyName("propertyName")] public string? PropertyName { get; init; }
    [JsonPropertyName("propertyValue")] public string? PropertyValue { get; init; }

    /// <summary>Object type: the part of the subscription type before the first dot (contact, company, lead).</summary>
    [JsonIgnore]
    public string ObjectType => SubscriptionType?.Split('.', 2)[0] ?? "";

    [JsonIgnore]
    public bool IsSupported =>
        SubscriptionType is not null
        && ObjectType is "contact" or "company" or "lead"
        && (SubscriptionType.EndsWith(".creation", StringComparison.Ordinal)
            || SubscriptionType.EndsWith(".propertyChange", StringComparison.Ordinal));
}
