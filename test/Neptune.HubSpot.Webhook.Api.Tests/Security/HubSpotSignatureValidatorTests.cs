using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Neptune.HubSpot.Webhook.Api.Configuration;
using Neptune.HubSpot.Webhook.Api.Security;

namespace Neptune.HubSpot.Webhook.Api.Tests.Security;

public class HubSpotSignatureValidatorTests
{
    private const string Path = "/webhooks/hubspot";
    private const string Body = "[{\"eventId\":1}]";
    private static readonly HubSpotOptions Config = new() { ClientSecret = "s3cret", PublicBaseUrl = "https://a.ngrok.app" };
    private readonly FakeTimeProvider _clock = new(DateTimeOffset.Parse("2026-01-01T00:00:00Z"));

    private HubSpotSignatureValidator CreateValidator() => new(Options.Create(Config), _clock);

    private string Timestamp(TimeSpan offset = default) =>
        _clock.GetUtcNow().Add(offset).ToUnixTimeMilliseconds().ToString();

    [Fact]
    public void Accepts_valid_signature()
    {
        var ts = Timestamp();
        var sig = HubSpotSignatureValidator.Compute(Config, "POST", Path, Body, ts);

        Assert.True(CreateValidator().IsValid("POST", Path, Body, sig, ts));
    }

    [Fact]
    public void Rejects_tampered_body()
    {
        var ts = Timestamp();
        var sig = HubSpotSignatureValidator.Compute(Config, "POST", Path, Body, ts);

        Assert.False(CreateValidator().IsValid("POST", Path, "[]", sig, ts));
    }

    [Fact]
    public void Rejects_stale_timestamp()
    {
        var ts = Timestamp(TimeSpan.FromMinutes(-6));
        var sig = HubSpotSignatureValidator.Compute(Config, "POST", Path, Body, ts);

        Assert.False(CreateValidator().IsValid("POST", Path, Body, sig, ts));
    }

    [Fact]
    public void Rejects_missing_headers()
    {
        Assert.False(CreateValidator().IsValid("POST", Path, Body, null, null));
    }

    [Fact]
    public void Signs_using_the_configured_public_url()
    {
        var ts = Timestamp();
        var otherUrl = new HubSpotOptions { ClientSecret = Config.ClientSecret, PublicBaseUrl = "https://b.ngrok.app" };
        var sigForOtherUrl = HubSpotSignatureValidator.Compute(otherUrl, "POST", Path, Body, ts);

        Assert.False(CreateValidator().IsValid("POST", Path, Body, sigForOtherUrl, ts));
    }
}
