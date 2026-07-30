using Synaptix.Analyzers;
using Synaptix.Analyzers.Tests;

public sealed class STBL0002Tests
{
    private static readonly Synaptix0002NoBannedParamAbbrevAnalyzer Analyzer = new();

    [Fact]
    public async Task FlagsMethodParamNamedCt()
    {
        const string source = "public sealed class Foo { public void Bar(string ct) { } }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0002", 0, 0, 0, 0, new[] { "ct" }));
    }

    [Fact]
    public async Task FlagsMethodParamNamedReq()
    {
        const string source = "public sealed class Foo { public void Bar(string req) { } }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0002", 0, 0, 0, 0, new[] { "req" }));
    }

    [Fact]
    public async Task DoesNotFlagNormalParamName()
    {
        const string source = "public sealed class Foo { public void Bar(string source) { } }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Theory]
    [InlineData("ct")]
    [InlineData("req")]
    [InlineData("resp")]
    [InlineData("err")]
    [InlineData("msg")]
    [InlineData("ex")]
    [InlineData("svc")]
    [InlineData("u")]
    [InlineData("x")]
    [InlineData("tmp")]
    public async Task FlagsAllBannedAbbreviations(string name)
    {
        string source = $"public sealed class Foo {{ public void Bar(string {name}) {{ }} }}";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0002", 0, 0, 0, 0, new[] { name }));
    }

    [Fact]
    public async Task FlagsOnlyBannedParamsInMultiParamMethod()
    {
        const string source = "public sealed class Foo { public void Bar(string good, string ct, int count) { } }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0002", 0, 0, 0, 0, new[] { "ct" }));
    }

    [Fact]
    public async Task FlagsConstructorParam()
    {
        const string source = "public sealed class Foo { public Foo(string msg) { } }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0002", 0, 0, 0, 0, new[] { "msg" }));
    }
}
