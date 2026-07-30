using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Synaptix.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Synaptix0009CollectionInitializerAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "STBL0009";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "STBL0009: Collection initializer can be a collection expression",
        messageFormat: "'new {0} {{ ... }}' can be a collection expression '[...]' (C# 12+).",
        category: AnalysisConstants.DiagnosticCategoryStyle,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Collection initializer expressions (`new T[] { ... }`, `new List<T> { ... }`, `new Dictionary<K, V> { ... }`) can be rewritten as C# 12 collection expressions (`[...]`). Collection expressions are shorter and read more uniformly across collection types.",
        helpLinkUri: AnalysisConstants.HelpLinkBase + "STBL0009-collection-initializer.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static compilationContext =>
        {
            if (compilationContext.Compilation is not CSharpCompilation csharpCompilation)
            {
                return;
            }

            var langVersion = csharpCompilation.LanguageVersion;
            if (langVersion is not (LanguageVersion.Default or >= LanguageVersion.CSharp12))
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                Analyze,
                SyntaxKind.ObjectCreationExpression);
        });
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ObjectCreationExpressionSyntax objectCreation)
        {
            return;
        }

        if (objectCreation.Initializer is null)
        {
            return;
        }

        if (objectCreation.Initializer.Expressions.Count == 0)
        {
            return;
        }

        if (objectCreation.Initializer.Kind() is not (
            SyntaxKind.ObjectInitializerExpression or SyntaxKind.CollectionInitializerExpression))
        {
            return;
        }

        var typeInfo = context.SemanticModel.GetTypeInfo(objectCreation, context.CancellationToken);
        if (typeInfo.Type is not INamedTypeSymbol namedType)
        {
            return;
        }

        if (!IsConvertibleTarget(namedType))
        {
            return;
        }

        var displayName = objectCreation.Type.ToString();
        var location = Location.Create(objectCreation.SyntaxTree, objectCreation.Span);
        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            location,
            displayName));
    }

    internal static bool IsConvertibleTarget(INamedTypeSymbol namedType)
    {
        if (namedType.TypeKind == TypeKind.Array)
        {
            return true;
        }

        var originalDefinition = namedType.OriginalDefinition;
        var fullName = originalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return fullName is
            "global::System.Collections.Generic.List<T>" or
            "global::System.Collections.Generic.Dictionary<TKey, TValue>" or
            "global::System.Collections.Generic.HashSet<T>" or
            "global::System.Collections.Generic.IList<T>" or
            "global::System.Collections.Generic.ICollection<T>" or
            "global::System.Collections.Generic.IEnumerable<T>" or
            "global::System.Collections.Generic.IReadOnlyList<T>" or
            "global::System.Collections.Generic.IReadOnlyCollection<T>";
    }
}
