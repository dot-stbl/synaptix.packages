using Synaptix.Analyzers;
using Synaptix.Analyzers.Tests;

public sealed class STBL0005CodeFixTests
{
    private static readonly Synaptix0005NoAwaitDiscardAnalyzer Analyzer = new();
    private static readonly Synaptix0005NoAwaitDiscardCodeFixProvider Fixer = new();

    [Fact]
    public async Task CodeFixRemovesDiscard()
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

        const string fixedSource = @"
public sealed class Foo
{
    public async Task Bar(System.Threading.CancellationToken ct)
    {
        await Task.Delay(1, ct);
    }
}
";

        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }
}
