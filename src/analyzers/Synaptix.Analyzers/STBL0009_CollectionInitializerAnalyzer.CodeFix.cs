using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Synaptix.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Synaptix0009CollectionInitializerCodeFixProvider)), Shared]
public sealed class Synaptix0009CollectionInitializerCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(Synaptix0009CollectionInitializerAnalyzer.DiagnosticId);

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

            var objectCreation = node as ObjectCreationExpressionSyntax
                ?? node.FirstAncestorOrSelf<ObjectCreationExpressionSyntax>();
            if (objectCreation is null)
            {
                continue;
            }

            if (!CanConvert(objectCreation))
            {
                continue;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Convert to collection expression",
                    createChangedDocument: ct => ConvertAsync(context.Document, objectCreation, ct),
                    equivalenceKey: "STBL0009_ConvertToCollectionExpression"),
                diagnostic);
        }

        await Task.CompletedTask;
    }

    internal static bool CanConvert(ObjectCreationExpressionSyntax objectCreation)
    {
        if (objectCreation.Initializer is null)
        {
            return false;
        }

        if (objectCreation.Initializer.Expressions.Count == 0)
        {
            return false;
        }

        if (objectCreation.Initializer.Kind() is not (
            SyntaxKind.ObjectInitializerExpression or SyntaxKind.CollectionInitializerExpression))
        {
            return false;
        }

        if (objectCreation.ArgumentList is not null && objectCreation.ArgumentList.Arguments.Count > 0)
        {
            return false;
        }

        foreach (var expression in objectCreation.Initializer.Expressions)
        {
            if (!IsSupportedElement(expression))
            {
                return false;
            }
        }

        return true;
    }

    internal static bool IsSupportedElement(ExpressionSyntax expression)
    {
        return expression.Kind() switch
        {
            SyntaxKind.NumericLiteralExpression => true,
            SyntaxKind.StringLiteralExpression => true,
            SyntaxKind.CharacterLiteralExpression => true,
            SyntaxKind.TrueLiteralExpression => true,
            SyntaxKind.FalseLiteralExpression => true,
            SyntaxKind.NullLiteralExpression => true,
            SyntaxKind.IdentifierName => true,
            SyntaxKind.SimpleMemberAccessExpression => true,
            SyntaxKind.InvocationExpression => true,
            SyntaxKind.ObjectCreationExpression => true,
            SyntaxKind.CastExpression => true,
            SyntaxKind.AsExpression => true,
            SyntaxKind.ConditionalExpression => true,
            SyntaxKind.CoalesceExpression => true,
            SyntaxKind.ParenthesizedExpression => true,
            SyntaxKind.UnaryPlusExpression => true,
            SyntaxKind.UnaryMinusExpression => true,
            SyntaxKind.LogicalNotExpression => true,
            SyntaxKind.BitwiseNotExpression => true,
            SyntaxKind.PreIncrementExpression => true,
            SyntaxKind.PreDecrementExpression => true,
            SyntaxKind.PostIncrementExpression => true,
            SyntaxKind.PostDecrementExpression => true,
            SyntaxKind.DefaultLiteralExpression => true,
            SyntaxKind.TypeOfExpression => true,
            SyntaxKind.SizeOfExpression => true,
            SyntaxKind.ImplicitArrayCreationExpression => true,
            SyntaxKind.CollectionExpression => true,
            SyntaxKind.ArrayCreationExpression => true,
            SyntaxKind.AnonymousObjectCreationExpression => true,
            SyntaxKind.AwaitExpression => true,
            _ => false,
        };
    }

    internal static ExpressionSyntax ExtractExpression(ExpressionSyntax expression)
    {
        return expression switch
        {
            InvocationExpressionSyntax
            {
                Expression: IdentifierNameSyntax { Identifier.Text: "Add" },
                ArgumentList.Arguments.Count: >= 1,
            } invocation => invocation.ArgumentList.Arguments[0].Expression,
            _ => expression,
        };
    }

    private static async Task<Document> ConvertAsync(
        Document document,
        ObjectCreationExpressionSyntax objectCreation,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var initializer = objectCreation.Initializer!;
        var elements = new List<CollectionElementSyntax>(initializer.Expressions.Count);

        foreach (var expression in initializer.Expressions)
        {
            elements.Add(SyntaxFactory.ExpressionElement(ExtractExpression(expression)));
        }

        var openBracket = SyntaxFactory.Token(SyntaxKind.OpenBracketToken);
        var closeBracket = SyntaxFactory.Token(SyntaxKind.CloseBracketToken);

        var collectionExpression = SyntaxFactory.CollectionExpression(
            openBracket,
            SyntaxFactory.SeparatedList(elements),
            closeBracket);

        var newCollectionExpression = collectionExpression
            .WithLeadingTrivia(objectCreation.GetLeadingTrivia())
            .WithTrailingTrivia(objectCreation.GetTrailingTrivia());

        var newRoot = root.ReplaceNode(objectCreation, newCollectionExpression);
        return document.WithSyntaxRoot(newRoot);
    }
}
