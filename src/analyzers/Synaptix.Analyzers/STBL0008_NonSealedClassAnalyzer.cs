using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Synaptix.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Synaptix0008NonSealedClassAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "STBL0008";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "STBL0008: Concrete class is not sealed",
        messageFormat: "Class '{0}' is not sealed. Per naming-and-types.md §2, concrete classes must be sealed.",
        category: AnalysisConstants.DiagnosticCategoryDesign,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Concrete classes must be sealed by default. The two exceptions are (1) abstract base classes, and (2) classes that have a documented extension point (existing derived class). CA1852 only fires on internal classes — this rule extends the check to public classes as well.",
        helpLinkUri: AnalysisConstants.HelpLinkBase + "STBL0008-non-sealed-class.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static compilationContext =>
        {
            if (compilationContext.Compilation is not CSharpCompilation)
            {
                return;
            }

            var allClasses = new List<INamedTypeSymbol>();
            CollectAllClasses(compilationContext.Compilation.GlobalNamespace, allClasses);

            compilationContext.RegisterSymbolAction(
                ctx => Analyze(ctx, allClasses),
                SymbolKind.NamedType);
        });
    }

    private static void CollectAllClasses(INamespaceOrTypeSymbol container, List<INamedTypeSymbol> result)
    {
        foreach (var member in container.GetTypeMembers())
        {
            CollectClassRecursive(member, result);
        }
    }

    private static void CollectClassRecursive(INamedTypeSymbol type, List<INamedTypeSymbol> result)
    {
        if (type.TypeKind == TypeKind.Class)
        {
            result.Add(type);
        }

        foreach (var nested in type.GetTypeMembers())
        {
            CollectClassRecursive(nested, result);
        }
    }

    private static void Analyze(SymbolAnalysisContext context, List<INamedTypeSymbol> allClasses)
    {
        if (context.Symbol is not INamedTypeSymbol classSymbol)
        {
            return;
        }

        if (classSymbol.TypeKind != TypeKind.Class)
        {
            return;
        }

        if (classSymbol.IsAbstract)
        {
            return;
        }

        if (classSymbol.IsSealed)
        {
            return;
        }

        if (classSymbol.IsStatic)
        {
            return;
        }

        var declaringSyntax = classSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(context.CancellationToken);
        if (declaringSyntax is not ClassDeclarationSyntax classDeclaration)
        {
            return;
        }

        if (classDeclaration.Modifiers.Any(SyntaxKind.SealedKeyword))
        {
            return;
        }

        if (classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword))
        {
            return;
        }

        if (classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return;
        }

        if (classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return;
        }

        foreach (var other in allClasses)
        {
            if (SymbolEqualityComparer.Default.Equals(other, classSymbol))
            {
                continue;
            }

            var baseType = other.BaseType;
            while (baseType is not null)
            {
                if (SymbolEqualityComparer.Default.Equals(baseType, classSymbol))
                {
                    return;
                }

                baseType = baseType.BaseType;
            }
        }

        var location = Location.Create(classDeclaration.SyntaxTree, classDeclaration.Identifier.Span);
        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            location,
            classSymbol.Name));
    }
}
