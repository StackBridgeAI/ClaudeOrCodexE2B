using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Neptune.HubSpot.Webhook.Api.Configuration;

namespace Neptune.HubSpot.Webhook.Api.Security;

/// <summary>Verifies HubSpot v3 signatures: base64(HMAC-SHA256(secret, method + url + body + timestamp)).</summary>
public sealed class HubSpotSignatureValidator(IOptions<HubSpotOptions> options, TimeProvider clock)
{
    public const string SignatureHeader = "X-HubSpot-Signature-v3";
    public const string TimestampHeader = "X-HubSpot-Request-Timestamp";
    private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(5);

    public bool IsValid(string method, string pathAndQuery, string body, string? signature, string? timestamp)
    {
        if (string.IsNullOrEmpty(signature)
            || !long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var timestampMs))
        {
            return false;
        }

        var age = clock.GetUtcNow() - DateTimeOffset.FromUnixTimeMilliseconds(timestampMs);
        if (age.Duration() > MaxAge)
        {
            return false;
        }

        var expected = Compute(options.Value, method, pathAndQuery, body, timestamp!);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(signature));
    }

    public static string Compute(HubSpotOptions config, string method, string pathAndQuery, string body, string timestamp)
    {
        var uri = config.PublicBaseUrl.TrimEnd('/') + pathAndQuery;
        var source = method.ToUpperInvariant() + uri + body + timestamp;
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(config.ClientSecret), Encoding.UTF8.GetBytes(source));
        return Convert.ToBase64String(hash);
    }
}
