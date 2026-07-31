using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Synaptix.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Synaptix0011NoCatchExceptionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "STBL0011";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "STBL0011: catch (Exception) outside top-level handler",
        messageFormat: "Avoid 'catch (Exception)'. Catch a specific exception type. Reserve 'catch (Exception)' for top-level entry points.",
        category: AnalysisConstants.DiagnosticCategoryDesign,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Catch-all 'catch (Exception)' (including bare 'catch' without a type) is reserved for top-level exception handlers. Catch the specific exception type you can act on. Per exceptions.md §2.",
        helpLinkUri: AnalysisConstants.HelpLinkBase + "STBL0011-no-catch-exception.md");

    private static readonly ImmutableHashSet<string> CatchAllTypeNames = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "System.Exception",
        "System.ApplicationException",
        "System.SystemException");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.CatchClause);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not CatchClauseSyntax catchClause)
        {
            return;
        }

        var filePath = catchClause.SyntaxTree.FilePath;
        if (filePath.EndsWith(".Designer.cs", StringComparison.Ordinal)
            || filePath.EndsWith(".g.cs", StringComparison.Ordinal)
            || filePath.EndsWith(".generated.cs", StringComparison.Ordinal))
        {
            return;
        }

        if (catchClause.Declaration is not null)
        {
            var caughtType = context.SemanticModel
                .GetTypeInfo(catchClause.Declaration.Type, context.CancellationToken).Type;

            if (caughtType is null)
            {
                return;
            }

            if (!CatchAllTypeNames.Contains(caughtType.ToDisplayString()))
            {
                return;
            }
        }

        var enclosingMethod = catchClause.FirstAncestorOrSelf<MethodDeclarationSyntax>();

        var obsoleteAttribute = context.Compilation.GetTypeByMetadataName("System.ObsoleteAttribute");
        if (enclosingMethod is not null
            && obsoleteAttribute is not null
            && HasAttribute(enclosingMethod.AttributeLists, obsoleteAttribute, context.SemanticModel))
        {
            return;
        }

        if (enclosingMethod is not null
            && HasTopLevelExceptionHandlerAttribute(enclosingMethod.AttributeLists))
        {
            return;
        }

        if (IsRethrowOnly(catchClause))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, catchClause.CatchKeyword.GetLocation()));
    }

    private static bool HasAttribute(
        SyntaxList<AttributeListSyntax> attributeLists,
        INamedTypeSymbol attributeType,
        SemanticModel semanticModel)
    {
        foreach (var list in attributeLists)
        {
            foreach (var attr in list.Attributes)
            {
                var symbol = semanticModel.GetSymbolInfo(attr).Symbol;
                if (symbol is null)
                {
                    continue;
                }

                if (SymbolEqualityComparer.Default.Equals(symbol.ContainingType, attributeType))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasTopLevelExceptionHandlerAttribute(SyntaxList<AttributeListSyntax> attributeLists)
    {
        foreach (var list in attributeLists)
        {
            foreach (var attr in list.Attributes)
            {
                var name = attr.Name.ToString();
                if (name is "TopLevelExceptionHandler" or "TopLevelExceptionHandlerAttribute")
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsRethrowOnly(CatchClauseSyntax catchClause)
    {
        if (catchClause.Block is null)
        {
            return false;
        }

        var statements = catchClause.Block.Statements;
        return statements.Count == 1
            && statements[0] is ThrowStatementSyntax throwStatement
            && throwStatement.Expression is null;
    }
}