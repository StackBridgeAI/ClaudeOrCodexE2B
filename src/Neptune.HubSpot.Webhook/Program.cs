using Neptune.HubSpot.Webhook.Handling;
using Neptune.HubSpot.Webhook.Security;
using Neptune.HubSpot.Webhook.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOptions<HubSpotOptions>()
    .BindConfiguration(HubSpotOptions.SectionName)
    .Validate(o => !string.IsNullOrWhiteSpace(o.ClientSecret), "HubSpot:ClientSecret must be configured.")
    .ValidateOnStart();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<HubSpotSignatureValidator>();
builder.Services.AddSingleton<IHubSpotEventHandler, LoggingHubSpotEventHandler>();
builder.Services.AddSingleton<HubSpotEventProcessor>();
builder.Services.AddControllers();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapControllers();

app.Run();

/// <summary>Entry-point marker for integration tests.</summary>
public partial class Program;
