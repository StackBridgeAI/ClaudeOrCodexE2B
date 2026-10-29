using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Neptune.HubSpot.Webhook.Api.Events;

namespace Neptune.HubSpot.Webhook.Api.Tests.Controllers;

public sealed class WebhookApiFactory(bool configureSecret = true) : WebApplicationFactory<Program>
{
    public const string Secret = "test-secret";
    public const string BaseUrl = "https://example.ngrok.app";

    public FakeTimeProvider Clock { get; } = new(DateTimeOffset.Parse("2026-01-01T00:00:00Z"));
    public ConcurrentQueue<WebhookEvent> Handled { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (configureSecret)
        {
            builder.UseSetting("HubSpot:ClientSecret", Secret);
            builder.UseSetting("HubSpot:PublicBaseUrl", BaseUrl);
        }

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IWebhookEventHandler>();
            foreach (var type in new[] { WebhookObjectType.Contact, WebhookObjectType.Company, WebhookObjectType.Lead })
            {
                services.AddSingleton<IWebhookEventHandler>(new RecordingHandler(type, Handled));
            }

            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }

    private sealed class RecordingHandler(WebhookObjectType objectType, ConcurrentQueue<WebhookEvent> handled)
        : IWebhookEventHandler
    {
        public WebhookObjectType ObjectType { get; } = objectType;

        public Task HandleAsync(WebhookEvent webhookEvent, CancellationToken cancellationToken)
        {
            handled.Enqueue(webhookEvent);
            return Task.CompletedTask;
        }
    }
}
