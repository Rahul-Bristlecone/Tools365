using System.Text;
using System.Text.Json;

namespace Tools365.Technology.JwtDecoder;

internal sealed record JwtDecodeResult(string Header, string Payload, string Signature);

internal static class JwtTokenDecoder
{
    private const int MaximumTokenLength = 1_048_576;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static JwtDecodeResult Decode(string token)
    {
        token = token.Trim();
        if (token.Length == 0)
        {
            throw new FormatException("Paste a JWT to decode.");
        }

        if (token.Length > MaximumTokenLength)
        {
            throw new FormatException("The token exceeds the 1 MB size limit.");
        }

        var parts = token.Split('.');
        if (parts.Length != 3 || parts[0].Length == 0 || parts[1].Length == 0)
        {
            throw new FormatException("A JWT must contain a non-empty header and payload separated into three dot-delimited parts.");
        }

        using var header = ParseJsonSegment(parts[0], "header");
        using var payload = ParseJsonSegment(parts[1], "payload");
        if (header.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("The JWT header must be a JSON object.");
        }

        if (payload.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("The JWT payload must be a JSON object.");
        }

        return new JwtDecodeResult(
            JsonSerializer.Serialize(header.RootElement, JsonOptions),
            JsonSerializer.Serialize(payload.RootElement, JsonOptions),
            parts[2].Length == 0 ? "(empty signature)" : parts[2]);
    }

    private static JsonDocument ParseJsonSegment(string segment, string name)
    {
        try
        {
            var bytes = Convert.FromBase64String(ToBase64(segment));
            return JsonDocument.Parse(StrictUtf8.GetString(bytes));
        }
        catch (FormatException exception)
        {
            throw new FormatException($"The JWT {name} is not valid base64url.", exception);
        }
        catch (DecoderFallbackException exception)
        {
            throw new FormatException($"The JWT {name} is not valid UTF-8.", exception);
        }
        catch (JsonException exception)
        {
            throw new FormatException($"The JWT {name} does not contain valid JSON.", exception);
        }
    }

    private static string ToBase64(string value)
    {
        if (value.Any(character =>
                !(char.IsAsciiLetterOrDigit(character) || character is '-' or '_')))
        {
            throw new FormatException("The JWT contains characters outside the base64url alphabet.");
        }

        var base64 = value.Replace('-', '+').Replace('_', '/');
        return (base64.Length % 4) switch
        {
            0 => base64,
            2 => base64 + "==",
            3 => base64 + "=",
            _ => throw new FormatException("The JWT contains an invalid base64url segment.")
        };
    }
}
