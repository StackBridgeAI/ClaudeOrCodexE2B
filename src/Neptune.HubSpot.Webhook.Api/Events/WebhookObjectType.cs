namespace Neptune.HubSpot.Webhook.Api.Events;

public enum WebhookObjectType
{
    Unknown = 0,
    Contact,
    Company,
    Lead,
}

public static class WebhookObjectTypeParser
{
    /// <summary>Maps e.g. "contact.creation" or "lead.propertyChange"; any other subscription is Unknown.</summary>
    public static WebhookObjectType Parse(string? subscriptionType)
    {
        if (string.IsNullOrEmpty(subscriptionType)
            || !(subscriptionType.EndsWith(".creation", StringComparison.Ordinal)
                 || subscriptionType.EndsWith(".propertyChange", StringComparison.Ordinal)))
        {
            return WebhookObjectType.Unknown;
        }

        return subscriptionType.Split('.')[0] switch
        {
            "contact" => WebhookObjectType.Contact,
            "company" => WebhookObjectType.Company,
            "lead" => WebhookObjectType.Lead,
            _ => WebhookObjectType.Unknown,
        };
    }
}
