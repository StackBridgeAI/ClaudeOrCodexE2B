using System.Net;
using System.Text;
using Neptune.HubSpot.Webhook.Api.Configuration;
using Neptune.HubSpot.Webhook.Api.Security;

namespace Neptune.HubSpot.Webhook.Api.Tests.Controllers;

/// <summary>In-process integration tests of POST /webhooks/hubspot.</summary>
public class HubSpotWebhookControllerTests : IDisposable
{
    private const string Path = "/webhooks/hubspot";
    private readonly WebhookApiFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    private HttpRequestMessage SignedRequest(string body, bool validSignature = true)
    {
        var timestamp = _factory.Clock.GetUtcNow().ToUnixTimeMilliseconds().ToString();
        var config = new HubSpotOptions { ClientSecret = WebhookApiFactory.Secret, PublicBaseUrl = WebhookApiFactory.BaseUrl };
        var signature = HubSpotSignatureValidator.Compute(config, "POST", Path, body, timestamp);

        var request = new HttpRequestMessage(HttpMethod.Post, Path)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        request.Headers.Add(HubSpotSignatureValidator.TimestampHeader, timestamp);
        request.Headers.Add(HubSpotSignatureValidator.SignatureHeader, validSignature ? signature : "invalid");
        return request;
    }

    private async Task WaitForHandledAsync(int count)
    {
        for (var i = 0; i < 100 && _factory.Handled.Count < count; i++)
        {
            await Task.Delay(50);
        }
    }

    [Fact]
    public async Task Signed_batch_is_acknowledged_and_events_reach_handlers()
    {
        const string body = """
            [{"eventId":1,"subscriptionType":"contact.creation","objectId":10,"occurredAt":1},
             {"eventId":2,"subscriptionType":"company.propertyChange","objectId":20,"occurredAt":2}]
            """;

        var response = await _factory.CreateClient().SendAsync(SignedRequest(body));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await WaitForHandledAsync(2);
        Assert.Equal(2, _factory.Handled.Count);
    }

    [Fact]
    public async Task Unknown_json_fields_are_tolerated()
    {
        const string body = """[{"eventId":3,"subscriptionType":"lead.creation","objectId":1,"extra":"x","portalId":9}]""";

        var response = await _factory.CreateClient().SendAsync(SignedRequest(body));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await WaitForHandledAsync(1);
        Assert.Single(_factory.Handled);
    }

    [Fact]
    public async Task Bad_signature_returns_401_and_no_handler_runs()
    {
        const string body = """[{"eventId":4,"subscriptionType":"contact.creation","objectId":1}]""";

        var response = await _factory.CreateClient().SendAsync(SignedRequest(body, validSignature: false));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await Task.Delay(200);
        Assert.Empty(_factory.Handled);
    }

    [Fact]
    public async Task Malformed_json_returns_400()
    {
        var response = await _factory.CreateClient().SendAsync(SignedRequest("{not json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Empty_batch_returns_200()
    {
        var response = await _factory.CreateClient().SendAsync(SignedRequest("[]"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(_factory.Handled);
    }
}
