using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Neptune.HubSpot.Webhook.Security;

namespace Neptune.HubSpot.Webhook.Tests;

public sealed class WebhookEndpointTests : IClassFixture<WebhookEndpointTests.Factory>
{
    private const string Secret = "test-secret";
    private const string BaseUrl = "http://localhost";

    private readonly Factory factory;

    public WebhookEndpointTests(Factory factory) => this.factory = factory;

    public sealed class Factory : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureHostConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HubSpot:ClientSecret"] = Secret,
                ["HubSpot:PublicBaseUrl"] = BaseUrl,
            }));
            return base.CreateHost(builder);
        }
    }

    private static string Event(long id, string type = "contact.creation") =>
        $$"""{"eventId":{{id}},"subscriptionId":1,"portalId":2,"occurredAt":1,"subscriptionType":"{{type}}","objectId":3}""";

    private async Task<HttpResponseMessage> PostAsync(string body, bool sign = true, long? timestampMs = null)
    {
        var timestamp = (timestampMs ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).ToString();
        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/hubspot")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        if (sign)
        {
            var signature = HubSpotSignatureValidator.Compute(Secret, "POST", BaseUrl + "/webhooks/hubspot", body, timestamp);
            request.Headers.Add(HubSpotSignatureValidator.SignatureHeader, signature);
            request.Headers.Add(HubSpotSignatureValidator.TimestampHeader, timestamp);
        }

        return await factory.CreateClient().SendAsync(request);
    }

    [Fact] // FR1.1, FR1.2, FR3.2
    public async Task ValidSignedBatch_Returns200AndProcessesEvents()
    {
        var response = await PostAsync($"[{Event(101)},{Event(102, "company.propertyChange")},{Event(103, "lead.creation")}]");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"processed\":3", await response.Content.ReadAsStringAsync());
    }

    [Fact] // FR2.2
    public async Task MissingSignature_Returns401()
    {
        var response = await PostAsync($"[{Event(201)}]", sign: false);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact] // FR2.2
    public async Task StaleTimestamp_Returns401()
    {
        var stale = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeMilliseconds();

        var response = await PostAsync($"[{Event(301)}]", timestampMs: stale);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact] // FR1.4
    public async Task UnsupportedType_IsAcknowledgedNotProcessed()
    {
        var response = await PostAsync($"[{Event(401, "deal.creation")}]");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"unsupported\":1", await response.Content.ReadAsStringAsync());
    }

    [Fact] // FR3.3
    public async Task RedeliveredEvent_IsNotProcessedTwice()
    {
        await PostAsync($"[{Event(501)}]");
        var second = await PostAsync($"[{Event(501)}]");

        Assert.Contains("\"duplicates\":1", await second.Content.ReadAsStringAsync());
    }

    [Fact] // FR3.4
    public async Task MalformedEvent_DoesNotBlockValidEvents()
    {
        var response = await PostAsync($"[{{\"eventId\":\"x\"}},{Event(601)}]");

        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"malformed\":1", json);
        Assert.Contains("\"processed\":1", json);
    }

    [Fact] // FR3.5
    public async Task InvalidJson_Returns400()
    {
        var response = await PostAsync("{not json");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact] // FR3.5
    public async Task EmptyArray_Returns200()
    {
        var response = await PostAsync("[]");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
