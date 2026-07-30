using Synaptix.Analyzers;
using Synaptix.Analyzers.Tests;

public sealed class STBL0003Tests
{
    private static readonly Synaptix0003NoPrivateMethodAnalyzer Analyzer = new();

    [Fact]
    public async Task FlagsPrivateInstanceMethod()
    {
        const string source = "public sealed class Foo { private void Helper() { } }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0003", 0, 0, 0, 0, new[] { "Helper" }));
    }

    [Fact]
    public async Task FlagsPrivateStaticMethod()
    {
        const string source = "public sealed class Foo { private static int Calc() { return 0; } }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0003", 0, 0, 0, 0, new[] { "Calc" }));
    }

    [Fact]
    public async Task DoesNotFlagPublicMethod()
    {
        const string source = "public sealed class Foo { public void PublicMethod() { } }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagOverrideMethod()
    {
        const string source = @"
public abstract class Base { public abstract void Do(); }
public sealed class Foo : Base { public override void Do() { } }
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagVirtualMethod()
    {
        const string source = "public class Foo { protected virtual void Bar() { } }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagParameterlessConstructor()
    {
        const string source = "public sealed class Foo { private Foo() { } }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagDisposeBoolPattern()
    {
        const string source = @"
public sealed class Foo : System.IDisposable
{
    private bool disposed;
    public void Dispose() { Dispose(disposing: true); }
    private void Dispose(bool disposing) { if (disposed) return; disposed = true; }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagPrivateMethodInAbstractClass()
    {
        const string source = @"
public abstract class Foo
{
    private void Helper() { }
    public abstract void Bar();
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagGeneratedCodeMethod()
    {
        const string source = @"
public sealed class Foo
{
    [System.CodeDom.Compiler.GeneratedCode(""tool"", ""1.0"")]
    private void Helper() { }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }
}
