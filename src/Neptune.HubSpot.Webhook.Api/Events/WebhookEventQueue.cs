using System.Threading.Channels;

namespace Neptune.HubSpot.Webhook.Api.Events;

/// <summary>Bounded in-memory queue so the controller can acknowledge before handlers run.</summary>
public sealed class WebhookEventQueue
{
    private readonly Channel<WebhookEvent> _channel = Channel.CreateBounded<WebhookEvent>(
        new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true });

    public bool TryEnqueue(WebhookEvent webhookEvent) => _channel.Writer.TryWrite(webhookEvent);

    public IAsyncEnumerable<WebhookEvent> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
