using System.Text.Json;

namespace Synaptix.Analyzers.Samples.Infrastructure;

public sealed class JsonPayloadBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public string BuildPayload(string eventType, object payload)
    {
        var envelope = new
        {
            eventType,
            payload,
            timestamp = DateTimeOffset.UtcNow,
        };

        return JsonSerializer.Serialize(envelope, JsonOptions);
    }

    public string BulkSerialize(IReadOnlyList<object> items)
    {
        var list = new List<object>(items.Count);
        foreach (var item in items)
        {
            list.Add(new { item });
        }

        return JsonSerializer.Serialize(list, JsonOptions);
    }

    private static string WrapWithMetadata(string json, string correlationId)
    {
        return $"{{\"correlationId\":\"{correlationId}\",\"body\":{json}}}";
    }

    private static bool IsJson(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        return (trimmed.StartsWith('{') && trimmed.EndsWith('}'))
            || (trimmed.StartsWith('[') && trimmed.EndsWith(']'));
    }
}
