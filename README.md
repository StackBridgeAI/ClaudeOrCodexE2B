# Neptune.HubSpot.Webhook

.NET 10 Aspire application with a controller-based API that receives HubSpot webhooks for contact, company and lead creation and property changes.

- `POST /webhooks/hubspot` verifies the HubSpot v3 signature (HMAC-SHA256, 5-minute window), queues the events and returns 200.
- Handlers implement `IWebhookEventHandler`; the default one logs ids only (no property values).

## Layout

```
src/Neptune.HubSpot.Webhook.Api              # the API
src/Neptune.HubSpot.Webhook.AppHost          # Aspire host
src/Neptune.HubSpot.Webhook.ServiceDefaults  # OpenTelemetry, health, resilience
test/Neptune.HubSpot.Webhook.Api.Tests       # xUnit tests
```

## Run locally with ngrok

Prerequisites: .NET 10 SDK, the Aspire tooling, [ngrok](https://ngrok.com/download) with an authtoken.

1. Start the tunnel to the pinned API port: `ngrok http 5080`. Copy the `https://…` forwarding URL.
2. Set the parameters (never commit them):
   ```
   cd src/Neptune.HubSpot.Webhook.AppHost
   dotnet user-secrets set "Parameters:hubspot-client-secret" "<HubSpot app client secret>"
   dotnet user-secrets set "Parameters:hubspot-public-base-url" "https://<your-id>.ngrok-free.app"
   ```
   The base URL must be exactly the public URL HubSpot calls, as it is part of the signed string.
3. Run `dotnet run --project src/Neptune.HubSpot.Webhook.AppHost`.
4. In your HubSpot app (Webhooks settings) set the target URL to `https://<your-id>.ngrok-free.app/webhooks/hubspot`, and subscribe to contact, company and lead creation and property-change events.
5. Create or edit a record in HubSpot. The API log shows `HubSpot event … occurred at …`.

A free ngrok URL changes on each restart: repeat steps 2 and 4 when it does.

## Test

```
dotnet test test/Neptune.HubSpot.Webhook.Api.Tests/Neptune.HubSpot.Webhook.Api.Tests.csproj
```
