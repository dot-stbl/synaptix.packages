using Synaptix.Analyzers;
using Synaptix.Analyzers.Tests;

public sealed class STBL0004CodeFixTests
{
    private static readonly Synaptix0004NoThrowIfNullAnalyzer Analyzer = new();
    private static readonly Synaptix0004NoThrowIfNullCodeFixProvider Fixer = new();

    [Fact]
    public async Task CodeFixReplacesThrowIfNull()
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

        const string fixedSource = @"
public sealed class Foo
{
    public void Bar(string x)
    {
        if (x is null) throw new System.ArgumentNullException(nameof(x));
    }
}
";

        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }

    [Fact]
    public async Task CodeFixReplacesThrowIfNullOrEmpty()
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

        const string fixedSource = @"
public sealed class Foo
{
    public void Bar(string s)
    {
        if (s is null) throw new System.ArgumentNullException(nameof(s));
    }
}
";

        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }

    [Fact]
    public async Task CodeFixReplacesArgumentExceptionThrowIfNullOrEmpty()
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

        const string fixedSource = @"
public sealed class Foo
{
    public void Bar(string s)
    {
        if (s is null) throw new System.ArgumentNullException(nameof(s));
    }
}
";

        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }

    [Fact]
    public async Task CodeFixPreservesFollowingStatements()
    {
        const string source = @"
public sealed class Foo
{
    public void Bar(string x)
    {
        System.ArgumentNullException.ThrowIfNull(x);
        System.Console.WriteLine(x);
    }
}
";

        const string fixedSource = @"
public sealed class Foo
{
    public void Bar(string x)
    {
        if (x is null) throw new System.ArgumentNullException(nameof(x));
        System.Console.WriteLine(x);
    }
}
";

        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }
}
