# Neptune.HubSpot.Webhook

.NET 10 / Aspire API that receives HubSpot webhook events (Contacts, Companies, Leads created or modified).

## Layout

- `src/Neptune.HubSpot.Webhook` – controller-based API (`POST /webhooks/hubspot`)
- `src/Neptune.HubSpot.Webhook.ServiceDefaults` – Aspire telemetry, health, resilience
- `src/Neptune.HubSpot.Webhook.AppHost` – Aspire orchestrator
- `test/Neptune.HubSpot.Webhook.Tests` – xunit tests

## Prerequisites

- .NET 10 SDK, Aspire workload-free (SDK-based) tooling
- [ngrok](https://ngrok.com/download) with an auth token (`ngrok config add-authtoken <token>`)
- HubSpot app client secret

## Run locally

```bash
dotnet user-secrets --project src/Neptune.HubSpot.Webhook.AppHost set Parameters:hubspot-client-secret "<client secret>"
dotnet run --project src/Neptune.HubSpot.Webhook.AppHost
```

The Aspire dashboard shows the `hubspot-webhook` resource and its HTTP endpoint (port in the dashboard).
In a second terminal expose that port:

```bash
ngrok http <api-port>
```

HubSpot signs the public URL, so set it to the Ngrok URL (no trailing path), for example in the AppHost
user-secrets or environment: `HubSpot__PublicBaseUrl=https://<id>.ngrok-free.app`. Then point the HubSpot app
webhook target URL to `https://<id>.ngrok-free.app/webhooks/hubspot`.

## Test

```bash
dotnet test test/Neptune.HubSpot.Webhook.Tests
```
