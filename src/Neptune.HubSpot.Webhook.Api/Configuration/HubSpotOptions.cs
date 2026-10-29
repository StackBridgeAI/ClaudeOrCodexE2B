using System.ComponentModel.DataAnnotations;

namespace Neptune.HubSpot.Webhook.Api.Configuration;

public sealed class HubSpotOptions
{
    public const string SectionName = "HubSpot";

    /// <summary>HubSpot app client secret, used to verify webhook signatures. Never logged.</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "HubSpot:ClientSecret is required.")]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Public base URL HubSpot calls (the ngrok URL locally); part of the signed string.</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "HubSpot:PublicBaseUrl is required.")]
    public string PublicBaseUrl { get; set; } = string.Empty;
}
