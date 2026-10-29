var builder = DistributedApplication.CreateBuilder(args);

var clientSecret = builder.AddParameter("hubspot-client-secret", secret: true);

builder.AddProject<Projects.Neptune_HubSpot_Webhook>("hubspot-webhook")
    .WithEnvironment("HubSpot__ClientSecret", clientSecret)
    .WithExternalHttpEndpoints();

builder.Build().Run();
