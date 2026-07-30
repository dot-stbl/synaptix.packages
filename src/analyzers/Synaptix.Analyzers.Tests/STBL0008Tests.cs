using Synaptix.Analyzers;
using Synaptix.Analyzers.Tests;

public sealed class STBL0008Tests
{
    private static readonly Synaptix0008NonSealedClassAnalyzer Analyzer = new();
    private static readonly Synaptix0008NonSealedClassCodeFixProvider Fixer = new();

    [Fact]
    public async Task FlagsNonSealedPublicClass()
    {
        const string source = "public class Foo { }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0008", 0, 0, 0, 0, new[] { "Foo" }));
    }

    [Fact]
    public async Task DoesNotFlagSealedClass()
    {
        const string source = "public sealed class Foo { }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagAbstractClass()
    {
        const string source = "public abstract class Foo { }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagClassWithDerivedType()
    {
        const string source = "public class Base { } public sealed class Derived : Base { }";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task CodeFixAddsSealedModifier()
    {
        const string source = "public class Foo { }";
        const string fixedSource = "public sealed class Foo { }";
        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }
}
