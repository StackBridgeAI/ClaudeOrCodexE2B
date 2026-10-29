namespace Neptune.HubSpot.Webhook.Security;

public sealed class HubSpotOptions
{
    public const string SectionName = "HubSpot";

    /// <summary>HubSpot app client secret used to verify v3 signatures. Keep in user-secrets.</summary>
    public string ClientSecret { get; set; } = "";

    /// <summary>
    /// Public base URL (for example the Ngrok URL). HubSpot signs the public URL, so when running
    /// behind a tunnel or proxy this overrides the host seen by Kestrel. Optional.
    /// </summary>
    public string PublicBaseUrl { get; set; } = "";

    public int MaxTimestampAgeSeconds { get; set; } = 300;
}
