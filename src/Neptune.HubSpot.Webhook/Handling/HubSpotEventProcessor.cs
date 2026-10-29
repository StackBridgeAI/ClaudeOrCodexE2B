using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Neptune.HubSpot.Webhook.Models;

namespace Neptune.HubSpot.Webhook.Handling;

public sealed record ProcessingResult(int Processed, int Duplicates, int Unsupported, int Malformed);

/// <summary>Parses a batch, skips duplicates by event id, and hands supported events to the handler.</summary>
public sealed partial class HubSpotEventProcessor(
    IHubSpotEventHandler handler, IMemoryCache seenEvents, ILogger<HubSpotEventProcessor> logger)
{
    private static readonly TimeSpan SeenTtl = TimeSpan.FromHours(24);

    /// <param name="batch">A JSON array element. The caller has already checked it is an array.</param>
    public async Task<ProcessingResult> ProcessAsync(JsonElement batch, CancellationToken cancellationToken)
    {
        int processed = 0, duplicates = 0, unsupported = 0, malformed = 0;

        foreach (var element in batch.EnumerateArray())
        {
            var hubSpotEvent = TryParse(element);
            if (hubSpotEvent is null)
            {
                malformed++;
                LogMalformed();
                continue;
            }

            if (!hubSpotEvent.IsSupported)
            {
                unsupported++;
                LogUnsupported(hubSpotEvent.EventId, hubSpotEvent.SubscriptionType);
                continue;
            }

            // Claim the event id before handling so a concurrent redelivery is not handled twice.
            if (seenEvents.TryGetValue(hubSpotEvent.EventId, out _))
            {
                duplicates++;
                continue;
            }

            seenEvents.Set(hubSpotEvent.EventId, true, SeenTtl);
            try
            {
                await handler.HandleAsync(hubSpotEvent, cancellationToken);
                processed++;
            }
            catch
            {
                seenEvents.Remove(hubSpotEvent.EventId); // let HubSpot's retry reprocess it
                throw;
            }
        }

        return new ProcessingResult(processed, duplicates, unsupported, malformed);
    }

    private static HubSpotEvent? TryParse(JsonElement element)
    {
        try
        {
            var parsed = element.Deserialize<HubSpotEvent>();
            return parsed is { EventId: not 0, SubscriptionType: not null } ? parsed : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Skipped malformed HubSpot event in batch")]
    private partial void LogMalformed();

    [LoggerMessage(Level = LogLevel.Information, Message = "Ignored unsupported HubSpot event {EventId} of type {SubscriptionType}")]
    private partial void LogUnsupported(long eventId, string? subscriptionType);
}
