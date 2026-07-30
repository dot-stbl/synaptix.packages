using Synaptix.Analyzers;
using Synaptix.Analyzers.Tests;

public sealed class STBL0002CodeFixTests
{
    private static readonly Synaptix0002NoBannedParamAbbrevAnalyzer Analyzer = new();
    private static readonly Synaptix0002NoBannedParamAbbrevCodeFixProvider Fixer = new();

    [Fact]
    public async Task CodeFixRenamesCt()
    {
        const string source = "public sealed class Foo { public void Bar(string ct) { } }";
        const string fixedSource = "public sealed class Foo { public void Bar(string cancellationToken) { } }";
        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }

    [Fact]
    public async Task CodeFixRenamesReq()
    {
        const string source = "public sealed class Foo { public void Bar(string req) { } }";
        const string fixedSource = "public sealed class Foo { public void Bar(string request) { } }";
        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }

    [Fact]
    public async Task CodeFixRenamesResp()
    {
        const string source = "public sealed class Foo { public void Bar(string resp) { } }";
        const string fixedSource = "public sealed class Foo { public void Bar(string response) { } }";
        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }

    [Fact]
    public async Task CodeFixRenamesErr()
    {
        const string source = "public sealed class Foo { public void Bar(string err) { } }";
        const string fixedSource = "public sealed class Foo { public void Bar(string error) { } }";
        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }

    [Fact]
    public async Task CodeFixRenamesMsg()
    {
        const string source = "public sealed class Foo { public void Bar(string msg) { } }";
        const string fixedSource = "public sealed class Foo { public void Bar(string message) { } }";
        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }

    [Fact]
    public async Task CodeFixRenamesSvc()
    {
        const string source = "public sealed class Foo { public void Bar(string svc) { } }";
        const string fixedSource = "public sealed class Foo { public void Bar(string service) { } }";
        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }

    [Fact]
    public async Task CodeFixRenamesU()
    {
        const string source = "public sealed class Foo { public void Bar(string u) { } }";
        const string fixedSource = "public sealed class Foo { public void Bar(string user) { } }";
        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }

    [Fact]
    public async Task CodeFixRenamesTmp()
    {
        const string source = "public sealed class Foo { public void Bar(string tmp) { } }";
        const string fixedSource = "public sealed class Foo { public void Bar(string temporary) { } }";
        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }

    [Fact]
    public async Task CodeFixRenamesConstructorParameter()
    {
        const string source = "public sealed class Foo { public Foo(string msg) { } }";
        const string fixedSource = "public sealed class Foo { public Foo(string message) { } }";
        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }
}
