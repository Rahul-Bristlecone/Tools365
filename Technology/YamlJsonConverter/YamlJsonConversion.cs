using System.Text.Json;
using YamlDotNet.Serialization;

namespace Tools365.Technology.YamlJsonConverter;

internal static class YamlJsonConversion
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithAttemptingUnquotedStringTypeDeserialization()
        .Build();

    private static readonly ISerializer YamlSerializer = new SerializerBuilder()
        .JsonCompatible()
        .Build();

    public static string YamlToJson(string yaml)
    {
        var value = YamlDeserializer.Deserialize<object?>(yaml);
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    public static string JsonToYaml(string json)
    {
        using var document = JsonDocument.Parse(json);
        var value = ConvertJsonElement(document.RootElement);
        return YamlSerializer.Serialize(value);
    }

    private static object? ConvertJsonElement(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.Object => ConvertJsonObject(element),
            JsonValueKind.Array => element.EnumerateArray()
                .Select(ConvertJsonElement)
                .ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt64(out var value) => value,
            JsonValueKind.Number when element.TryGetUInt64(out var value) => value,
            JsonValueKind.Number when element.TryGetDecimal(out var value) => value,
            JsonValueKind.Number => element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => throw new JsonException($"Unsupported JSON value: {element.ValueKind}.")
        };

    private static Dictionary<string, object?> ConvertJsonObject(JsonElement element)
    {
        var values = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            values[property.Name] = ConvertJsonElement(property.Value);
        }

        return values;
    }
}
