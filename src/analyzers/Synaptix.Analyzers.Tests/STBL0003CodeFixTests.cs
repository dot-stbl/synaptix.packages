using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Synaptix.Analyzers;
using Synaptix.Analyzers.Tests;

public sealed class STBL0003CodeFixTests
{
    private static readonly Synaptix0003NoPrivateMethodAnalyzer Analyzer = new();
    private static readonly Synaptix0003NoPrivateMethodCodeFixProvider Fixer = new();

    [Fact]
    public async Task CodeFixExtractsPrivateStaticMethodAsExtensionMethod()
    {
        const string source = "\r\npublic sealed class Foo\r\n{\r\n    private static int Calc() { return 0; }\r\n}\r\n";

        const string fixedSource = "\r\npublic sealed class Foo\r\n{\r\n}\r\n\r\nfile static class FooExtensions\r\n{\r\n    public static int Calc(this Foo self) { return 0; }\r\n}";

        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }

    [Fact]
    public async Task CodeFixMovesMethodAndAddsExtensionReceiver()
    {
        const string source = "\r\npublic sealed class Foo\r\n{\r\n    private static int Calc(int x, int y)\r\n    {\r\n        var sum = x + y;\r\n        return sum * 2;\r\n    }\r\n}\r\n";

        const string fixedSource = "\r\npublic sealed class Foo\r\n{\r\n}\r\n\r\nfile static class FooExtensions\r\n{\r\n    public static int Calc(this Foo self, int x, int y)\r\n    {\r\n        var sum = x + y;\r\n        return sum * 2;\r\n    }\r\n}";

        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }

    [Fact]
    public async Task CodeFixUpdatesUnqualifiedCallSitesToExtensionSyntax()
    {
        const string source = "\r\npublic sealed class Foo\r\n{\r\n    private static int Calc() { return 0; }\r\n    public int Use() { return Calc() + 1; }\r\n}\r\n";

        const string fixedSource = "\r\npublic sealed class Foo\r\n{\r\n    public int Use() { return this.Calc() + 1; }\r\n}\r\n\r\nfile static class FooExtensions\r\n{\r\n    public static int Calc(this Foo self) { return 0; }\r\n}";

        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }

    [Fact]
    public async Task CodeFixPrependsThisClassParameter()
    {
        const string source = "\r\npublic sealed class OrderService\r\n{\r\n    private static string FormatOrderId(int id) { return id.ToString(); }\r\n}\r\n";

        const string fixedSource = "\r\npublic sealed class OrderService\r\n{\r\n}\r\n\r\nfile static class OrderServiceExtensions\r\n{\r\n    public static string FormatOrderId(this OrderService self, int id) { return id.ToString(); }\r\n}";

        await AnalyzerTestRunner.VerifyCodeFixAsync(Analyzer, Fixer, source, fixedSource);
    }

    [Fact]
    public async Task CodeFixSkipsInstanceMethod()
    {
        const string source = "\r\npublic sealed class Foo\r\n{\r\n    private int Calc() { return 0; }\r\n}\r\n";

        var (_, diagnostics) = await AnalyzerTestRunner.RunAnalyzerAsync(Analyzer, source);
        var diag = diagnostics.First(d => d.Id == Synaptix0003NoPrivateMethodAnalyzer.DiagnosticId);

        var actions = new List<CodeAction>();
        var solution = AnalyzerTestRunner.CreateSolution(source);
        var document = solution.Projects.First().Documents.First();
        var context = new CodeFixContext(
            document, diag, (a, _) => actions.Add(a), CancellationToken.None);
        await Fixer.RegisterCodeFixesAsync(context);

        actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task CodeFixSkipsMethodWithOverload()
    {
        const string source = "\r\npublic sealed class Foo\r\n{\r\n    private static int Calc() { return 0; }\r\n    private static int Calc(int x) { return x; }\r\n}\r\n";

        var actions = await CollectCodeFixActionsAsync(source);

        actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task CodeFixSkipsMethodWithMethodImplAttribute()
    {
        const string source = "\r\npublic sealed class Foo\r\n{\r\n    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]\r\n    private static int Calc() { return 0; }\r\n}\r\n";

        var actions = await CollectCodeFixActionsAsync(source);

        actions.ShouldBeEmpty();
    }

    [Fact]
    public async Task CodeFixSkipsWhenExtensionClassAlreadyExists()
    {
        const string source = "\r\npublic sealed class Foo\r\n{\r\n    private static int Calc() { return 0; }\r\n}\r\n\r\nfile static class FooExtensions\r\n{\r\n    public static int Other() { return 1; }\r\n}\r\n";

        var actions = await CollectCodeFixActionsAsync(source);

        actions.ShouldBeEmpty();
    }

    private static async Task<List<CodeAction>> CollectCodeFixActionsAsync(string source)
    {
        var (_, diagnostics) = await AnalyzerTestRunner.RunAnalyzerAsync(Analyzer, source);
        var diag = diagnostics.First(d => d.Id == Synaptix0003NoPrivateMethodAnalyzer.DiagnosticId);

        var actions = new List<CodeAction>();
        var solution = AnalyzerTestRunner.CreateSolution(source);
        var document = solution.Projects.First().Documents.First();
        var context = new CodeFixContext(
            document, diag, (a, _) => actions.Add(a), CancellationToken.None);
        await Fixer.RegisterCodeFixesAsync(context);

        return actions;
    }
}