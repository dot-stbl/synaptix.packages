using System.Collections.Generic;

namespace Synaptix.Analyzers.Samples.Legacy;

public class DatabaseConnector
{
    private readonly string _connectionString;
    private readonly Synaptix.Analyzers.Samples.Common.ILogger _logger;
    private static int _connectionCounter;

    public DatabaseConnector(string connectionString, Synaptix.Analyzers.Samples.Common.ILogger logger)
    {
        _connectionString = connectionString;
        _logger = logger;
        _connectionCounter++;
    }

    public async Task OpenAsync(CancellationToken ct = default)
    {
        await Task.Delay(10, ct);
    }

    public void Close()
    {
    }

    #region Helpers

    private static string NormalizeConnectionString(string raw)
    {
        return raw.Trim();
    }

    private static bool IsValidConnectionString(string value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Contains('=');
    }

    #endregion

    private static List<string> ParseParameters(string connectionString)
    {
        var parts = new List<string>();
        foreach (var segment in connectionString.Split(';'))
        {
            parts.Add(segment.Trim());
        }
        return parts;
    }
}

internal class InternalServiceWithPublicMethod
{
    public void DoSomething()
    {
    }

    public int Compute(int value)
    {
        return value * 2;
    }
}
