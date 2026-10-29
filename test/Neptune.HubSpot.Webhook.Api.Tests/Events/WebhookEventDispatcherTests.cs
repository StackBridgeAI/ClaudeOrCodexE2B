using Microsoft.Extensions.Logging.Abstractions;
using Neptune.HubSpot.Webhook.Api.Events;

namespace Neptune.HubSpot.Webhook.Api.Tests.Events;

public class WebhookEventDispatcherTests
{
    [Theory]
    [InlineData("contact.creation", WebhookObjectType.Contact)]
    [InlineData("contact.propertyChange", WebhookObjectType.Contact)]
    [InlineData("company.creation", WebhookObjectType.Company)]
    [InlineData("company.propertyChange", WebhookObjectType.Company)]
    [InlineData("lead.creation", WebhookObjectType.Lead)]
    [InlineData("lead.propertyChange", WebhookObjectType.Lead)]
    public async Task Routes_event_to_handler_of_its_object_type(string subscriptionType, WebhookObjectType expected)
    {
        var handlers = new[] { WebhookObjectType.Contact, WebhookObjectType.Company, WebhookObjectType.Lead }
            .Select(t => new RecordingHandler(t)).ToList();
        var dispatcher = new WebhookEventDispatcher(handlers, NullLogger<WebhookEventDispatcher>.Instance);

        await dispatcher.DispatchAsync(new WebhookEvent { EventId = 1, SubscriptionType = subscriptionType }, default);

        var handled = Assert.Single(handlers, h => h.Handled.Count == 1);
        Assert.Equal(expected, handled.ObjectType);
    }

    [Fact]
    public async Task Ignores_unknown_subscription_type()
    {
        var handler = new RecordingHandler(WebhookObjectType.Contact);
        var dispatcher = new WebhookEventDispatcher([handler], NullLogger<WebhookEventDispatcher>.Instance);

        await dispatcher.DispatchAsync(new WebhookEvent { EventId = 2, SubscriptionType = "deal.creation" }, default);
        await dispatcher.DispatchAsync(new WebhookEvent { EventId = 3, SubscriptionType = "contact.deletion" }, default);

        Assert.Empty(handler.Handled);
    }

    [Fact]
    public async Task Same_event_id_twice_is_harmless()
    {
        var handler = new RecordingHandler(WebhookObjectType.Contact);
        var dispatcher = new WebhookEventDispatcher([handler], NullLogger<WebhookEventDispatcher>.Instance);
        var duplicate = new WebhookEvent { EventId = 4, SubscriptionType = "contact.creation" };

        await dispatcher.DispatchAsync(duplicate, default);
        await dispatcher.DispatchAsync(duplicate, default);

        Assert.Equal(2, handler.Handled.Count);
    }

    private sealed class RecordingHandler(WebhookObjectType objectType) : IWebhookEventHandler
    {
        public WebhookObjectType ObjectType { get; } = objectType;
        public List<WebhookEvent> Handled { get; } = [];

        public Task HandleAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken)
        {
            Handled.Add(webhookEvent);
            return Task.CompletedTask;
        }
    }
}
