using Synaptix.Analyzers;
using Synaptix.Analyzers.Tests;

public sealed class STBL0011Tests
{
    private static readonly Synaptix0011NoCatchExceptionAnalyzer Analyzer = new();

    [Fact]
    public async Task FlagsCatchException()
    {
        const string source = @"
public sealed class Foo
{
    public void Bar()
    {
        try { var x = 1; }
        catch (System.Exception) { }
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0011", 0, 0, 0, 0, Array.Empty<string>()));
    }

    [Fact]
    public async Task FlagsCatchWithoutType()
    {
        const string source = @"
public sealed class Foo
{
    public void Bar()
    {
        try { var x = 1; }
        catch { }
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0011", 0, 0, 0, 0, Array.Empty<string>()));
    }

    [Fact]
    public async Task FlagsCatchApplicationException()
    {
        const string source = @"
public sealed class Foo
{
    public void Bar()
    {
        try { var x = 1; }
        catch (System.ApplicationException) { }
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0011", 0, 0, 0, 0, Array.Empty<string>()));
    }

    [Fact]
    public async Task FlagsCatchSystemException()
    {
        const string source = @"
public sealed class Foo
{
    public void Bar()
    {
        try { var x = 1; }
        catch (System.SystemException) { }
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0011", 0, 0, 0, 0, Array.Empty<string>()));
    }

    [Fact]
    public async Task DoesNotFlagCatchSpecificType()
    {
        const string source = @"
public sealed class Foo
{
    public void Bar()
    {
        try { var x = 1; }
        catch (System.ArgumentException) { }
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagCatchWithRethrowOnly()
    {
        const string source = @"
public sealed class Foo
{
    public void Bar()
    {
        try { var x = 1; }
        catch (System.Exception) { throw; }
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagInObsoleteMethod()
    {
        const string source = @"
public sealed class Foo
{
    [System.Obsolete]
    public void Bar()
    {
        try { var x = 1; }
        catch (System.Exception) { }
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagInTopLevelHandler()
    {
        const string source = @"
public sealed class TopLevelExceptionHandlerAttribute : System.Attribute { }

public sealed class Foo
{
    [TopLevelExceptionHandler]
    public void Bar()
    {
        try { var x = 1; }
        catch (System.Exception) { }
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagInGeneratedCode()
    {
        const string source = @"
[System.CodeDom.Compiler.GeneratedCode(""tool"", ""1.0"")]
public sealed class Foo
{
    public void Bar()
    {
        try { var x = 1; }
        catch (System.Exception) { }
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }
}