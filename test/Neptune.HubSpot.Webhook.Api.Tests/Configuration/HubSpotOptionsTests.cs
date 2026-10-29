using Neptune.HubSpot.Webhook.Api.Tests.Controllers;

namespace Neptune.HubSpot.Webhook.Api.Tests.Configuration;

public class HubSpotOptionsTests
{
    [Fact]
    public void Missing_secret_fails_startup_naming_the_key()
    {
        using var factory = new WebhookApiFactory(configureSecret: false);

        var error = Record.Exception(() => factory.CreateClient());

        Assert.NotNull(error);
        Assert.Contains("HubSpot:ClientSecret", error.ToString());
    }
}
