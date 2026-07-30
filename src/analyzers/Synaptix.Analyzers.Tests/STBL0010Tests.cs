using Synaptix.Analyzers;
using Synaptix.Analyzers.Tests;

public sealed class STBL0010Tests
{
    private static readonly Synaptix0010NoRegionDirectiveAnalyzer Analyzer = new();

    [Fact]
    public async Task FlagsRegionDirective()
    {
        const string source = "public sealed class Foo { #region Methods public void Bar() { } #endregion }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0010", 0, 0, 0, 0, Array.Empty<string>()));
    }

    [Fact]
    public async Task DoesNotFlagNonRegionCode()
    {
        const string source = "public sealed class Foo { public void Bar() { } }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }
}
