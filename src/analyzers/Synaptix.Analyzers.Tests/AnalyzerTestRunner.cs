using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Shouldly;

namespace Synaptix.Analyzers.Tests;

/// <summary>
/// Minimal in-tree test framework for Roslyn analyzers/code fixes.
/// Avoids the legacy 1.1.x testing packages (which pin Roslyn 1.0 and break
/// on net8.0) and runs the analyzer/fixer directly against a synthetic
/// compilation.
/// </summary>
internal static class AnalyzerTestRunner
{
    public static async Task VerifyAnalyzerAsync(
        DiagnosticAnalyzer analyzer,
        string source,
        params (string Id, int StartLine, int StartColumn, int EndLine, int EndColumn, string[] Arguments)[] expected)
    {
        var (compilation, diagnostics) = await RunAnalyzerAsync(analyzer, source);

        var compErrors = diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        compErrors.ShouldBeEmpty(
            $"Compilation errors:\n{string.Join("\n", compErrors.Select(d => d.ToString()))}");

        var analyzerId = GetAnalyzerId(analyzer);
        var analyzerDiagnostics = diagnostics
            .Where(d => d.Id == analyzerId && d.Location.IsInSource)
            .ToList();

        analyzerDiagnostics.Count.ShouldBe(
            expected.Length,
            $"Expected {expected.Length} diagnostic(s) but got {analyzerDiagnostics.Count}. "
            + $"All diagnostics:\n{string.Join("\n", diagnostics.Select(d => d.ToString()))}");

        for (var i = 0; i < expected.Length; i++)
        {
            var e = expected[i];
            var actual = analyzerDiagnostics[i];

            actual.Id.ShouldBe(e.Id);

            var span = actual.Location.GetLineSpan();
            if (e.StartLine > 0)
            {
                span.StartLinePosition.Line.ShouldBe(
                    e.StartLine - 1,
                    $"Diagnostic {i} start line: expected {e.StartLine - 1}, actual {span.StartLinePosition.Line}. "
                    + $"Full span: {span}");
            }

            if (e.StartColumn > 0)
            {
                span.StartLinePosition.Character.ShouldBe(
                    e.StartColumn - 1,
                    $"Diagnostic {i} start column: expected {e.StartColumn - 1}, actual {span.StartLinePosition.Character}. "
                    + $"Full span: {span}");
            }

            if (e.EndLine > 0)
            {
                span.EndLinePosition.Line.ShouldBe(
                    e.EndLine - 1,
                    $"Diagnostic {i} end line: expected {e.EndLine - 1}, actual {span.EndLinePosition.Line}. "
                    + $"Full span: {span}");
            }

            if (e.EndColumn > 0)
            {
                span.EndLinePosition.Character.ShouldBe(
                    e.EndColumn - 1,
                    $"Diagnostic {i} end column: expected {e.EndColumn - 1}, actual {span.EndLinePosition.Character}. "
                    + $"Full span: {span}");
            }

            foreach (var arg in e.Arguments)
            {
                actual.GetMessage().ShouldContain(arg);
            }
        }
    }

    public static async Task VerifyCodeFixAsync(
        DiagnosticAnalyzer analyzer,
        CodeFixProvider fixer,
        string source,
        string fixedSource,
        int codeActionIndex = 0)
    {
        var (_, diagnostics) = await RunAnalyzerAsync(analyzer, source);

        var analyzerId = GetAnalyzerId(analyzer);
        var analyzerDiagnostic = diagnostics.FirstOrDefault(d => d.Id == analyzerId && d.Location.IsInSource);
        analyzerDiagnostic.ShouldNotBeNull();

        var solution = CreateSolution(source);
        var document = solution.Projects.First().Documents.First();

        var actions = new List<CodeAction>();
        var context = new CodeFixContext(
            document,
            analyzerDiagnostic,
            (action, _) => actions.Add(action),
            CancellationToken.None);

        await fixer.RegisterCodeFixesAsync(context);

        actions.Count.ShouldBeGreaterThan(codeActionIndex);
        var action = actions[codeActionIndex];

        var operations = await action.GetOperationsAsync(CancellationToken.None);
        var newSolution = ((ApplyChangesOperation)operations.First()).ChangedSolution;
        var newDoc = newSolution.Projects.First().Documents.First();
        var newText = (await newDoc.GetTextAsync()).ToString();

        newText.ShouldBe(fixedSource);
    }

    private static async Task<(CSharpCompilation Compilation, ImmutableArray<Diagnostic> Diagnostics)> RunAnalyzerAsync(
        DiagnosticAnalyzer analyzer,
        string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new List<MetadataReference>();

        var trustedAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")) ?? "";
        var trustedPaths = trustedAssemblies.Split(Path.PathSeparator);
        foreach (var path in trustedPaths)
        {
            var fileName = Path.GetFileName(path);
            if (fileName.StartsWith("System.", StringComparison.Ordinal)
                || fileName.StartsWith("Microsoft.", StringComparison.Ordinal)
                || fileName == "mscorlib.dll"
                || fileName == "netstandard.dll")
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        if (references.Count == 0)
        {
            var sdkDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
            if (sdkDir is not null)
            {
                foreach (var name in new[] { "System.Runtime.dll", "System.Console.dll", "System.Collections.dll" })
                {
                    var p = Path.Combine(sdkDir, name);
                    if (File.Exists(p))
                    {
                        references.Add(MetadataReference.CreateFromFile(p));
                    }
                }
            }
        }

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var withAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));
        var diagnostics = await withAnalyzers.GetAnalyzerDiagnosticsAsync();
        return (compilation, diagnostics);
    }

    private static Solution CreateSolution(string source)
    {
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);

        var references = new List<MetadataReference>();

        var trustedAssemblies = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")) ?? "";
        var trustedPaths = trustedAssemblies.Split(Path.PathSeparator);
        foreach (var path in trustedPaths)
        {
            var fileName = Path.GetFileName(path);
            if (fileName.StartsWith("System.", StringComparison.Ordinal)
                || fileName.StartsWith("Microsoft.", StringComparison.Ordinal)
                || fileName == "mscorlib.dll"
                || fileName == "netstandard.dll")
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        var projectInfo = ProjectInfo.Create(
            projectId,
            VersionStamp.Default,
            "TestProject",
            "TestProject",
            LanguageNames.CSharp,
            compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
            metadataReferences: references);

        return workspace.CurrentSolution
            .AddProject(projectInfo)
            .AddDocument(documentId, "Test.cs", SourceText.From(source));
    }

    private static string GetAnalyzerId(DiagnosticAnalyzer analyzer)
    {
        return analyzer.SupportedDiagnostics[0].Id;
    }
}
