using Synaptix.Analyzers;
using Synaptix.Analyzers.Tests;

public sealed class STBL0004Tests
{
    private static readonly Synaptix0004NoThrowIfNullAnalyzer Analyzer = new();

    [Fact]
    public async Task FlagsArgumentNullExceptionThrowIfNull()
    {
        const string source = @"
public sealed class Foo
{
    public void Bar(string x)
    {
        System.ArgumentNullException.ThrowIfNull(x);
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0004", 0, 0, 0, 0, Array.Empty<string>()));
    }

    [Fact]
    public async Task FlagsArgumentNullExceptionThrowIfNullOrEmpty()
    {
        const string source = @"
public sealed class Foo
{
    public void Bar(string s)
    {
        System.ArgumentNullException.ThrowIfNullOrEmpty(s);
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0004", 0, 0, 0, 0, Array.Empty<string>()));
    }

    [Fact]
    public async Task FlagsArgumentNullExceptionThrowIfNullOrWhiteSpace()
    {
        const string source = @"
public sealed class Foo
{
    public void Bar(string s)
    {
        System.ArgumentNullException.ThrowIfNullOrWhiteSpace(s);
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0004", 0, 0, 0, 0, Array.Empty<string>()));
    }

    [Fact]
    public async Task FlagsArgumentExceptionThrowIfNullOrEmpty()
    {
        const string source = @"
public sealed class Foo
{
    public void Bar(string s)
    {
        System.ArgumentException.ThrowIfNullOrEmpty(s);
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0004", 0, 0, 0, 0, Array.Empty<string>()));
    }

    [Fact]
    public async Task DoesNotFlagNullPatternCheck()
    {
        const string source = @"
public sealed class Foo
{
    public void Bar(string x)
    {
        if (x is null) throw new System.InvalidOperationException(""null"");
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagThrowIfNullInObsoleteMethod()
    {
        const string source = @"
public sealed class Foo
{
    [System.Obsolete]
    public void Bar(string x)
    {
        System.ArgumentNullException.ThrowIfNull(x);
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }
}
