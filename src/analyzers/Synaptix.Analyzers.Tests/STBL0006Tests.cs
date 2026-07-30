using Synaptix.Analyzers;
using Synaptix.Analyzers.Tests;

public sealed class STBL0006Tests
{
    private static readonly Synaptix0006TraditionalConstructorAnalyzer Analyzer = new();
    private static readonly Synaptix0006TraditionalConstructorCodeFixProvider Fixer = new();

    [Fact]
    public async Task FlagsTraditionalConstructorWithFieldAssignments()
    {
        const string source = @"
public sealed class OrderService
{
    private readonly int x;
    private readonly string y;

    public OrderService(int x, string y)
    {
        x = x;
        y = y;
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(
            Analyzer,
            source,
            ("STBL0006", 0, 0, 0, 0, new[] { "OrderService" }));
    }

    [Fact]
    public async Task DoesNotFlagPrimaryConstructor()
    {
        const string source = @"
public sealed class OrderService(int x, string y)
{
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagMultipleConstructors()
    {
        const string source = @"
public sealed class OrderService
{
    public OrderService() { }
    public OrderService(int x) { x = x; }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagStaticClass()
    {
        const string source = @"
public static class StaticHelpers
{
    static StaticHelpers() { }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagAbstractClass()
    {
        const string source = @"
public abstract class Base
{
    public Base(int x) { x = x; }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagComplexConstructorBody()
    {
        const string source = @"
public sealed class OrderService
{
    private readonly int x;
    private readonly string y;

    public OrderService(int x, string y)
    {
        if (x < 0)
        {
            throw new System.ArgumentOutOfRangeException(nameof(x));
        }
        this.x = x;
        this.y = y;
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagAssignmentFromNonParameter()
    {
        const string source = @"
public sealed class OrderService
{
    private readonly int x;

    public OrderService(int x)
    {
        this.x = x + 1;
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagMemberAccessAssignment()
    {
        const string source = @"
public sealed class OrderService
{
    private readonly Inner inner = new Inner();

    public OrderService(int x)
    {
        inner.Value = x;
    }

    private sealed class Inner
    {
        public int Value;
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagConstructorWithAttribute()
    {
        const string source = @"
public sealed class OrderService
{
    private readonly int x;

    [System.Obsolete]
    public OrderService(int x)
    {
        this.x = x;
    }
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task DoesNotFlagExpressionBodiedConstructor()
    {
        const string source = @"
public sealed class OrderService
{
    private readonly int x;

    public OrderService(int x) => this.x = x;
}
";
        await AnalyzerTestRunner.VerifyAnalyzerAsync(Analyzer, source);
    }

    [Fact]
    public async Task CodeFixConvertsToPrimaryConstructor()
    {
        const string source = "public sealed class OrderService { private readonly int x; private readonly string y; public OrderService(int x, string y) { x = x; y = y; } }";
        const string fixedSource = "public sealed class OrderService(int x, string y) { }";
        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }

    [Fact]
    public async Task CodeFixPreservesFieldsWithInitializers()
    {
        const string source = "public sealed class OrderService { private readonly int x; private readonly int cachedValue = 42; public OrderService(int x) { x = x; } }";

        string fixedSource = "public sealed class OrderService(int x) { private readonly int cachedValue = 42; }".Replace("\n", "\r\n");
        string sourceCrlf = source.Replace("\n", "\r\n");

        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, sourceCrlf, fixedSource);
    }

    [Fact]
    public async Task CodeFixKeepsFieldsWithUnderscoreNames()
    {
        const string source = "public sealed class OrderService { private readonly int _x; private readonly int _y; public OrderService(int x, int y) { _x = x; _y = y; } }";

        string fixedSource = "public sealed class OrderService(int x, int y) { private readonly int _x; private readonly int _y; }".Replace("\n", "\r\n");
        string sourceCrlf = source.Replace("\n", "\r\n");

        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, sourceCrlf, fixedSource);
    }
}
