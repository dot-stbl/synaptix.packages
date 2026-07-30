namespace Synaptix.Analyzers.Samples.Common;

public interface ILogger
{
    void LogInformation(string message, params object?[] args);
    void LogDebug(string message, params object?[] args);
    void LogWarning(string message, params object?[] args);
    void LogError(string message, params object?[] args);
}

public sealed class ConsoleLogger : ILogger
{
    public void LogInformation(string message, params object?[] args)
        => Console.WriteLine($"[INFO] " + string.Format(message, args));

    public void LogDebug(string message, params object?[] args)
        => Console.WriteLine($"[DEBUG] " + string.Format(message, args));

    public void LogWarning(string message, params object?[] args)
        => Console.WriteLine($"[WARN] " + string.Format(message, args));

    public void LogError(string message, params object?[] args)
        => Console.Error.WriteLine($"[ERROR] " + string.Format(message, args));
}

public interface ILogger<T>
{
    void LogInformation(string message, params object?[] args);
    void LogDebug(string message, params object?[] args);
    void LogWarning(string message, params object?[] args);
    void LogError(string message, params object?[] args);
}

public sealed class TypedConsoleLogger<T> : ILogger<T>
{
    public void LogInformation(string message, params object?[] args)
        => Console.WriteLine($"[INFO {typeof(T).Name}] " + string.Format(message, args));

    public void LogDebug(string message, params object?[] args)
        => Console.WriteLine($"[DEBUG {typeof(T).Name}] " + string.Format(message, args));

    public void LogWarning(string message, params object?[] args)
        => Console.WriteLine($"[WARN {typeof(T).Name}] " + string.Format(message, args));

    public void LogError(string message, params object?[] args)
        => Console.Error.WriteLine($"[ERROR {typeof(T).Name}] " + string.Format(message, args));
}
