namespace Synaptix.Analyzers.Samples.Infrastructure;

public sealed class CacheManager
{
    private readonly Dictionary<string, object> _store = new();

    public void Set(string key, object value)
    {
        _store[key] = value;
    }

    public object? Get(string key)
    {
        return _store.TryGetValue(key, out var value) ? value : null;
    }

    public IReadOnlyList<string> Keys()
    {
        return _store.Keys.ToList();
    }

    public void Clear()
    {
        _store.Clear();
    }

    public IReadOnlyList<string> AllowedTags { get; } = new List<string>
    {
        "primary",
        "secondary",
        "fallback",
    };
}

public sealed class StaticConfig
{
    public static readonly IReadOnlyList<string> Hosts = new List<string>
    {
        "prod-1.example.com",
        "prod-2.example.com",
        "prod-3.example.com",
    };

    public static readonly Dictionary<string, int> Limits = new Dictionary<string, int>
    {
        { "users", 1000 },
        { "requests", 5000 },
    };
}
