using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Synaptix.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Synaptix0002NoBannedParamAbbrevAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "STBL0002";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "STBL0002: Parameter name uses banned abbreviation",
        messageFormat: "Parameter '{0}' uses banned abbreviation. Use a full name. Banned: ct, req, resp, err, msg, ex, svc, u, x, tmp.",
        category: AnalysisConstants.DiagnosticCategoryNaming,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Parameter names must use full descriptive names. Single-letter or abbreviated names (ct, req, resp, err, msg, ex, svc, u, x, tmp) reduce readability.",
        helpLinkUri: AnalysisConstants.HelpLinkBase + "STBL0002-no-banned-param-abbrev.md");

    private static readonly ImmutableHashSet<string> BannedAbbreviations = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "ct",
        "req",
        "resp",
        "err",
        "msg",
        "ex",
        "svc",
        "u",
        "x",
        "tmp");

    public static readonly ImmutableDictionary<string, string> ExpansionMap =
        ImmutableDictionary.CreateRange(StringComparer.Ordinal,
            new KeyValuePair<string, string>[]
            {
                new("ct", "cancellationToken"),
                new("req", "request"),
                new("resp", "response"),
                new("err", "error"),
                new("msg", "message"),
                new("ex", "exception"),
                new("svc", "service"),
                new("u", "user"),
                new("tmp", "temporary"),
            });

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static compilationContext =>
        {
            compilationContext.RegisterSymbolAction(
                AnalyzeMethod,
                SymbolKind.Method);

            compilationContext.RegisterSymbolAction(
                AnalyzeNamedType,
                SymbolKind.NamedType);
        });
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;

        if (method.MethodKind is MethodKind.Constructor or MethodKind.StaticConstructor)
        {
            return;
        }

        ReportIfBanned(context, method.Parameters);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;

        if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct or TypeKind.Interface))
        {
            return;
        }

        foreach (var ctor in type.Constructors)
        {
            if (ctor.IsImplicitlyDeclared)
            {
                continue;
            }

            ReportIfBanned(context, ctor.Parameters);
        }
    }

    private static void ReportIfBanned(
        SymbolAnalysisContext context,
        IEnumerable<IParameterSymbol> parameters)
    {
        foreach (var parameter in parameters)
        {
            var name = parameter.Name;
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            if (!BannedAbbreviations.Contains(name))
            {
                continue;
            }

            foreach (var location in parameter.Locations)
            {
                if (!location.IsInSource)
                {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(
                    Rule,
                    location,
                    name));
            }
        }
    }
}
