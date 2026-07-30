using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Synaptix.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Synaptix0005NoAwaitDiscardAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "STBL0005";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "STBL0005: Discard over await",
        messageFormat: "Avoid `_ = await ...`. Use bare await or attach an explicit fire-and-forget comment.",
        category: AnalysisConstants.DiagnosticCategoryUsage,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The `_ = await ...` pattern is a misuse of discard — `await` already waits synchronously. Use a bare `await` statement, or document the fire-and-forget nature explicitly.",
        helpLinkUri: AnalysisConstants.HelpLinkBase + "STBL0005-no-await-discard.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(
            Analyze,
            SyntaxKind.ExpressionStatement);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ExpressionStatementSyntax statement)
        {
            return;
        }

        if (statement.Expression is not AssignmentExpressionSyntax assignment)
        {
            return;
        }

        if (assignment.Kind() != SyntaxKind.SimpleAssignmentExpression)
        {
            return;
        }

        if (assignment.Left is not IdentifierNameSyntax identifier)
        {
            return;
        }

        if (identifier.Identifier.Text != "_")
        {
            return;
        }

        if (assignment.Right is not AwaitExpressionSyntax)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            statement.GetLocation()));
    }
}
