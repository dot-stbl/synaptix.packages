using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Synaptix.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Synaptix0004NoThrowIfNullAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "STBL0004";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "STBL0004: ArgumentNullException.ThrowIfNull in non-boundary code",
        messageFormat: "Replace {0} with `is null` pattern matching. `<Nullable>enable</Nullable>` already enforces non-null at the call site.",
        category: AnalysisConstants.DiagnosticCategoryUsage,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "ArgumentNullException.ThrowIfNull* / ArgumentException.ThrowIfNull* are redundant in a non-nullable context — the compiler enforces non-null at the call site. Use pattern matching or a domain exception instead. Exception: boundary code (reflection, interop, serialization) where the signature cannot be trusted.",
        helpLinkUri: AnalysisConstants.HelpLinkBase + "STBL0004-no-throw-if-null.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var argumentNullExceptionType = compilationContext.Compilation.GetTypeByMetadataName(
                "System.ArgumentNullException");
            var argumentExceptionType = compilationContext.Compilation.GetTypeByMetadataName(
                "System.ArgumentException");
            var obsoleteAttributeType = compilationContext.Compilation.GetTypeByMetadataName(
                "System.ObsoleteAttribute");
            var excludeFromCodeCoverageAttributeType = compilationContext.Compilation.GetTypeByMetadataName(
                "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute");

            if (argumentNullExceptionType is null && argumentExceptionType is null)
            {
                return;
            }

            compilationContext.RegisterOperationAction(
                ctx => Analyze(ctx, argumentNullExceptionType, argumentExceptionType, obsoleteAttributeType, excludeFromCodeCoverageAttributeType),
                OperationKind.Invocation);
        });
    }

    private static void Analyze(
        OperationAnalysisContext context,
        INamedTypeSymbol? argumentNullExceptionType,
        INamedTypeSymbol? argumentExceptionType,
        INamedTypeSymbol? obsoleteAttributeType,
        INamedTypeSymbol? excludeFromCodeCoverageAttributeType)
    {
        if (context.Operation is not IInvocationOperation invocation)
        {
            return;
        }

        var targetMethod = invocation.TargetMethod;
        if (targetMethod.MethodKind != MethodKind.Ordinary)
        {
            return;
        }

        var methodName = targetMethod.Name;
        if (!methodName.StartsWith("ThrowIfNull", StringComparison.Ordinal))
        {
            return;
        }

        var containingType = targetMethod.ContainingType;
        if (containingType is null)
        {
            return;
        }

        var isArgumentNullException = argumentNullExceptionType is not null
            && SymbolEqualityComparer.Default.Equals(containingType, argumentNullExceptionType);
        var isArgumentException = argumentExceptionType is not null
            && SymbolEqualityComparer.Default.Equals(containingType, argumentExceptionType);

        if (!isArgumentNullException && !isArgumentException)
        {
            return;
        }

        var containingSymbol = context.ContainingSymbol;
        if (containingSymbol is IMethodSymbol containingMethod)
        {
            if (obsoleteAttributeType is not null
                && HasAttribute(containingMethod, obsoleteAttributeType))
            {
                return;
            }

            if (excludeFromCodeCoverageAttributeType is not null
                && HasAttribute(containingMethod, excludeFromCodeCoverageAttributeType))
            {
                return;
            }
        }

        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            invocation.Syntax.GetLocation(),
            $"{containingType.Name}.{methodName}"));
    }

    private static bool HasAttribute(IMethodSymbol method, INamedTypeSymbol attributeType)
    {
        return method.GetAttributes().Any(attr =>
            SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeType));
    }
}
