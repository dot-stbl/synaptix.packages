using Synaptix.Analyzers;
using Synaptix.Analyzers.Tests;

public sealed class STBL0007Tests
{
    private static readonly Synaptix0007NoUnderscoreFieldPrefixAnalyzer Analyzer = new();
    private static readonly Synaptix0007NoUnderscoreFieldPrefixCodeFixProvider Fixer = new();

    [Fact]
    public async Task FlagsUnderscoreField()
    {
        const string source = "public sealed class Foo { private int _foo; }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0007", 0, 0, 0, 0, new[] { "_foo" }));
    }

    [Fact]
    public async Task FlagsUnderscoreReadonlyField()
    {
        const string source = "public sealed class Foo { private readonly string _bar; }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0007", 0, 0, 0, 0, new[] { "_bar" }));
    }

    [Fact]
    public async Task FlagsUnderscoreStaticField()
    {
        const string source = "public sealed class Foo { private static System.Collections.Generic.List<int> _list = new(); }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0007", 0, 0, 0, 0, new[] { "_list" }));
    }

    [Fact]
    public async Task DoesNotFlagNormalField()
    {
        const string source = "public sealed class Foo { private int foo; }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task CodeFixRenamesFieldAndUpdatesReferences()
    {
        const string source = "public sealed class Foo { private int _foo; public int Bar() => _foo; }";
        const string fixedSource = "public sealed class Foo { private int foo; public int Bar() => foo; }";
        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }
}
