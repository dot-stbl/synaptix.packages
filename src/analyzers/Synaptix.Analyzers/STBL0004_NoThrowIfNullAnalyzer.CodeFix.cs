using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Synaptix.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Synaptix0004NoThrowIfNullCodeFixProvider)), Shared]
public sealed class Synaptix0004NoThrowIfNullCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(Synaptix0004NoThrowIfNullAnalyzer.DiagnosticId);

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

        foreach (var diagnostic in context.Diagnostics)
        {
            var span = diagnostic.Location.SourceSpan;
            var node = root.FindNode(span, getInnermostNodeForTie: true);

            var invocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation is null)
            {
                continue;
            }

            if (!invocation.ArgumentList.Arguments.Any())
            {
                continue;
            }

            var arg = invocation.ArgumentList.Arguments[0];
            var argName = (arg.Expression as IdentifierNameSyntax)?.Identifier.Text;
            if (argName is null)
            {
                continue;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Replace with `is null` check",
                    createChangedDocument: ct => ApplyFixAsync(
                        context.Document,
                        invocation,
                        argName,
                        ct),
                    equivalenceKey: "STBL0004_ReplaceWithIsNullCheck"),
                diagnostic);
        }

        await Task.CompletedTask;
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        string argName,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var expressionStatement = invocation.FirstAncestorOrSelf<ExpressionStatementSyntax>();
        if (expressionStatement is null)
        {
            return document;
        }

        _ = SyntaxFactory.IsPatternExpression(
            SyntaxFactory.IdentifierName(argName),
            SyntaxFactory.ConstantPattern(
                SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)));
        _ = SyntaxFactory.ObjectCreationExpression(
            SyntaxFactory.ParseTypeName("System.ArgumentNullException"))
            .WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Argument(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.IdentifierName("nameof"),
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.IdentifierName(argName)))))))));

        var ifStatement = (IfStatementSyntax)SyntaxFactory.ParseStatement(
            $"if ({argName} is null) throw new System.ArgumentNullException(nameof({argName}));")
            .WithLeadingTrivia(expressionStatement.GetLeadingTrivia())
            .WithTrailingTrivia(expressionStatement.GetTrailingTrivia());

        var newRoot = root.ReplaceNode(expressionStatement, ifStatement);
        return document.WithSyntaxRoot(newRoot);
    }
}
