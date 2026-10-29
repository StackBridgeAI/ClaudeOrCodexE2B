using Microsoft.Extensions.DependencyInjection.Extensions;
using Neptune.HubSpot.Webhook.Api.Configuration;
using Neptune.HubSpot.Webhook.Api.Events;
using Neptune.HubSpot.Webhook.Api.Security;
using Neptune.HubSpot.Webhook.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOptions<HubSpotOptions>()
    .BindConfiguration(HubSpotOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddControllers();
builder.Services.TryAddSingleton(TimeProvider.System);
builder.Services.AddSingleton<HubSpotSignatureValidator>();
builder.Services.AddSingleton<WebhookEventQueue>();
builder.Services.AddSingleton<WebhookEventDispatcher>();
builder.Services.AddHostedService<WebhookDispatchService>();

foreach (var type in new[] { WebhookObjectType.Contact, WebhookObjectType.Company, WebhookObjectType.Lead })
{
    builder.Services.AddSingleton<IWebhookEventHandler>(sp =>
        new LoggingWebhookEventHandler(type, sp.GetRequiredService<ILogger<LoggingWebhookEventHandler>>()));
}

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapControllers();

app.Run();

// Exposed for WebApplicationFactory<Program> in the test project.
public partial class Program;
