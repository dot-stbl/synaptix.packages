using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Synaptix.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Synaptix0001NoBannedSuffixesAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "STBL0001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "STBL0001: Type name uses banned suffix",
        messageFormat: "Type '{0}' uses banned suffix '{1}'. Use a role-specific suffix or none. Banned: Dto, Model, Impl, Util, Utility, ViewModel.",
        category: AnalysisConstants.DiagnosticCategoryNaming,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Type names must use role-specific suffixes (Request, Response, Summary, Detail, Service, Manager, Helper) or none. Generic-category suffixes (Dto, Model, Impl, Util, Utility, ViewModel) describe the implementation pattern, not the role.",
        helpLinkUri: AnalysisConstants.HelpLinkBase + "STBL0001-no-banned-suffixes.md");

    internal static readonly ImmutableHashSet<string> BannedSuffixes = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "Dto",
        "Model",
        "Impl",
        "Util",
        "Utility",
        "ViewModel");

    public static readonly ImmutableArray<string> BannedSuffixesPublic = BannedSuffixes.ToImmutableArray();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(
            Analyze,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.InterfaceDeclaration);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeDeclaration)
        {
            return;
        }

        var identifier = typeDeclaration.Identifier;
        var name = identifier.Text;

        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        foreach (var suffix in BannedSuffixes)
        {
            if (name.Length > suffix.Length
                && name.EndsWith(suffix, StringComparison.Ordinal))
            {
                var location = Location.Create(typeDeclaration.SyntaxTree, identifier.Span);
                context.ReportDiagnostic(Diagnostic.Create(
                    Rule,
                    location,
                    name,
                    suffix));
                return;
            }
        }
    }
}
