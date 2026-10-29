var builder = DistributedApplication.CreateBuilder(args);

// Set via user-secrets: Parameters:hubspot-client-secret / Parameters:hubspot-public-base-url
var clientSecret = builder.AddParameter("hubspot-client-secret", secret: true);
var publicBaseUrl = builder.AddParameter("hubspot-public-base-url");

// The http endpoint is pinned to 5080 so `ngrok http 5080` has a stable target.
builder.AddProject<Projects.Neptune_HubSpot_Webhook_Api>("hubspot-webhook-api")
    .WithEndpoint("http", endpoint => endpoint.Port = 5080)
    .WithEnvironment("HubSpot__ClientSecret", clientSecret)
    .WithEnvironment("HubSpot__PublicBaseUrl", publicBaseUrl);

builder.Build().Run();
