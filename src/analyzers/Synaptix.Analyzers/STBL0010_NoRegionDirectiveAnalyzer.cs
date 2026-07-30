using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Synaptix.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Synaptix0010NoRegionDirectiveAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "STBL0010";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "STBL0010: #region directive is banned",
        messageFormat: "#region directives are banned. Use member organization + XML doc instead.",
        category: AnalysisConstants.DiagnosticCategoryDesign,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "#region/#endregion directives are banned. They hide structure from IDE outline view and encourage god-class sprawl. Use member ordering, partial-by-responsibility splits, or extracted helpers instead.",
        helpLinkUri: AnalysisConstants.HelpLinkBase + "STBL0010-no-region-directive.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxTreeAction(AnalyzeTree);
    }

    private static void AnalyzeTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        foreach (var trivia in root.DescendantTrivia())
        {
            if (!trivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
                && !trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                Rule,
                Location.Create(context.Tree, trivia.Span)));
        }
    }
}
