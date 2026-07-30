using Synaptix.Analyzers;
using Synaptix.Analyzers.Tests;

public sealed class STBL0005Tests
{
    private static readonly Synaptix0005NoAwaitDiscardAnalyzer Analyzer = new();

    [Fact]
    public async Task FlagsDiscardOverAwait()
    {
        const string source = @"
public sealed class Foo
{
    public async Task Bar(System.Threading.CancellationToken ct)
    {
        _ = await Task.Delay(1, ct);
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0005", 0, 0, 0, 0, Array.Empty<string>()));
    }

    [Fact]
    public async Task DoesNotFlagBareAwait()
    {
        const string source = @"
public sealed class Foo
{
    public async Task Bar(System.Threading.CancellationToken ct)
    {
        await Task.Delay(1, ct);
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagAssignmentToVariable()
    {
        const string source = @"
public sealed class Foo
{
    public async Task<int> Bar(System.Threading.CancellationToken ct)
    {
        var x = await Task.FromResult(1);
        return x;
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagDiscardOverNonAwait()
    {
        const string source = @"
public sealed class Foo
{
    public void Bar()
    {
        _ = System.Console.Out;
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }
}
