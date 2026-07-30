using System.Collections.Generic;
using Synaptix.Analyzers;
using Synaptix.Analyzers.Tests;

public sealed class STBL0009Tests
{
    private static readonly Synaptix0009CollectionInitializerAnalyzer Analyzer = new();
    private static readonly Synaptix0009CollectionInitializerCodeFixProvider Fixer = new();

    [Fact]
    public async Task FlagsListInitializer()
    {
        const string source = "public sealed class Foo { System.Collections.Generic.List<int> Bar() => new System.Collections.Generic.List<int> { 1, 2 }; }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0009", 0, 0, 0, 0, new[] { "new System.Collections.Generic.List<int>" }));
    }

    [Fact]
    public async Task FlagsDictionaryInitializer()
    {
        const string source = "public sealed class Foo { System.Collections.Generic.Dictionary<string, int> Bar() => new System.Collections.Generic.Dictionary<string, int> { [\"a\"] = 1 }; }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0009", 0, 0, 0, 0, new[] { "new System.Collections.Generic.Dictionary<string, int>" }));
    }

    [Fact]
    public async Task DoesNotFlagEmptyInitializer()
    {
        const string source = "public sealed class Foo { System.Collections.Generic.List<int> Bar() => new System.Collections.Generic.List<int>(); }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task CodeFixConvertsToCollectionExpression()
    {
        const string source = "public sealed class Foo { System.Collections.Generic.List<int> Bar() => new System.Collections.Generic.List<int> { 1, 2 }; }";
        const string fixedSource = "public sealed class Foo { System.Collections.Generic.List<int> Bar() => [1, 2]; }";
        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }
}
