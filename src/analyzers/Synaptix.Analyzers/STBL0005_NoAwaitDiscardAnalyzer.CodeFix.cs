using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Synaptix.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Synaptix0005NoAwaitDiscardCodeFixProvider)), Shared]
public sealed class Synaptix0005NoAwaitDiscardCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(Synaptix0005NoAwaitDiscardAnalyzer.DiagnosticId);

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        if (node is not ExpressionStatementSyntax statement)
        {
            return;
        }

        if (statement.Expression is not AssignmentExpressionSyntax assignment)
        {
            return;
        }

        if (assignment.Right is not AwaitExpressionSyntax awaitExpression)
        {
            return;
        }

        var awaitStatement = SyntaxFactory.ExpressionStatement(awaitExpression)
            .WithLeadingTrivia(statement.GetLeadingTrivia())
            .WithTrailingTrivia(statement.GetTrailingTrivia());

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Remove discard",
                createChangedDocument: ct => RemoveDiscardAsync(context.Document, statement, awaitStatement, ct),
                equivalenceKey: "STBL0005_RemoveDiscard"),
            diagnostic);

        await Task.CompletedTask;
    }

    private static async Task<Document> RemoveDiscardAsync(
        Document document,
        ExpressionStatementSyntax original,
        ExpressionStatementSyntax replacement,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var newRoot = root.ReplaceNode(original, replacement);
        return document.WithSyntaxRoot(newRoot);
    }
}
