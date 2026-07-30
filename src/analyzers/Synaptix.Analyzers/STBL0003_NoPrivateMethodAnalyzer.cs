using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Synaptix.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Synaptix0003NoPrivateMethodAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "STBL0003";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "STBL0003: Private method in production class",
        messageFormat: "Method '{0}' is private. Extract to a file-scoped static helper class.",
        category: AnalysisConstants.DiagnosticCategoryDesign,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Private methods in production classes are procedural style. Extract to a file-scoped static helper class (or a separate type with its own dependencies).",
        helpLinkUri: AnalysisConstants.HelpLinkBase + "STBL0003-no-private-method.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;

        if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct))
        {
            return;
        }

        if (type.IsAbstract)
        {
            return;
        }

        var generatedCodeAttributeType = context.Compilation.GetTypeByMetadataName(
            "System.CodeDom.Compiler.GeneratedCodeAttribute");

        foreach (var member in type.GetMembers().OfType<IMethodSymbol>())
        {
            if (member.DeclaredAccessibility != Accessibility.Private)
            {
                continue;
            }

            if (member.MethodKind is not (MethodKind.Ordinary or MethodKind.Constructor or MethodKind.StaticConstructor))
            {
                continue;
            }

            if (member.IsImplicitlyDeclared)
            {
                continue;
            }

            if (member.IsOverride || member.IsVirtual)
            {
                continue;
            }

            if (IsParameterlessConstructor(member))
            {
                continue;
            }

            if (IsDisposeBoolPattern(member))
            {
                continue;
            }

            if (HasGeneratedCodeAttribute(member, generatedCodeAttributeType))
            {
                continue;
            }

            if (HasGeneratedCodeAttribute(type, generatedCodeAttributeType))
            {
                continue;
            }

            if (IsInGeneratedFile(member))
            {
                continue;
            }

            var containingLocation = member.Locations.FirstOrDefault(static l => l.IsInSource);
            if (containingLocation is null)
            {
                continue;
            }

            var methodDecl = member.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
            var nameSpan = methodDecl switch
            {
                MethodDeclarationSyntax m => m.Identifier.Span,
                ConstructorDeclarationSyntax c => c.Identifier.Span,
                _ => containingLocation.SourceSpan,
            };
            var location = Location.Create(containingLocation.SourceTree!, nameSpan);

            context.ReportDiagnostic(Diagnostic.Create(
                Rule,
                location,
                member.Name));
        }
    }

    private static bool IsParameterlessConstructor(IMethodSymbol method)
    {
        return method.MethodKind == MethodKind.Constructor
            && method.Parameters.Length == 0;
    }

    private static bool IsDisposeBoolPattern(IMethodSymbol method)
    {
        return method.Name == "Dispose"
            && method.Parameters.Length == 1
            && method.Parameters[0].Type.SpecialType == SpecialType.System_Boolean;
    }

    private static bool HasGeneratedCodeAttribute(ISymbol symbol, INamedTypeSymbol? generatedCodeAttributeType)
    {
        return generatedCodeAttributeType is not null
            && symbol.GetAttributes().Any(attr =>
                SymbolEqualityComparer.Default.Equals(attr.AttributeClass, generatedCodeAttributeType));
    }

    private static bool IsInGeneratedFile(ISymbol symbol)
    {
        foreach (var location in symbol.Locations)
        {
            if (!location.IsInSource)
            {
                continue;
            }

            var path = location.SourceTree?.FilePath;
            if (path is null)
            {
                continue;
            }

            var fileName = Path.GetFileName(path);
            if (fileName.EndsWith(".Designer.cs", StringComparison.Ordinal)
                || fileName.EndsWith(".g.cs", StringComparison.Ordinal)
                || fileName.EndsWith(".generated.cs", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
