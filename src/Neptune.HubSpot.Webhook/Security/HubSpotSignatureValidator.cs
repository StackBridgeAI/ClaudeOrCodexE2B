using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Neptune.HubSpot.Webhook.Security;

/// <summary>Validates the HubSpot v3 request signature (HMAC-SHA256 over method + uri + body + timestamp).</summary>
public sealed class HubSpotSignatureValidator(IOptions<HubSpotOptions> options, TimeProvider clock)
{
    public const string SignatureHeader = "X-HubSpot-Signature-v3";
    public const string TimestampHeader = "X-HubSpot-Request-Timestamp";

    public bool IsValid(string method, string requestUri, string body, string? signature, string? timestamp)
    {
        if (string.IsNullOrEmpty(signature)
            || !long.TryParse(timestamp, out var timestampMs))
        {
            return false;
        }

        var age = clock.GetUtcNow() - DateTimeOffset.FromUnixTimeMilliseconds(timestampMs);
        if (age.Duration() > TimeSpan.FromSeconds(options.Value.MaxTimestampAgeSeconds))
        {
            return false;
        }

        var expected = Compute(options.Value.ClientSecret, method, requestUri, body, timestamp!);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(signature));
    }

    public static string Compute(string secret, string method, string requestUri, string body, string timestamp)
    {
        var source = Encoding.UTF8.GetBytes(method + requestUri + body + timestamp);
        return Convert.ToBase64String(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), source));
    }
}
