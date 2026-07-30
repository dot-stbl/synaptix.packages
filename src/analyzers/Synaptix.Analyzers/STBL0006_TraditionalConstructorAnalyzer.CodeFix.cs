using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Synaptix.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Synaptix0006TraditionalConstructorCodeFixProvider)), Shared]
public sealed class Synaptix0006TraditionalConstructorCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(Synaptix0006TraditionalConstructorAnalyzer.DiagnosticId);

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

            if (node is not ClassDeclarationSyntax classDeclaration)
            {
                continue;
            }

            var ctors = classDeclaration.Members
                .OfType<ConstructorDeclarationSyntax>()
                .ToList();

            if (ctors.Count != 1)
            {
                continue;
            }

            var ctor = ctors[0];
            if (ctor.Body is null)
            {
                continue;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Convert to primary constructor",
                    createChangedDocument: ct => ConvertToPrimaryConstructorAsync(
                        context.Document,
                        classDeclaration,
                        ctor,
                        ct),
                    equivalenceKey: "STBL0006_ConvertToPrimaryConstructor"),
                diagnostic);
        }

        await Task.CompletedTask;
    }

    private static async Task<Document> ConvertToPrimaryConstructorAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        ConstructorDeclarationSyntax ctor,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var parameters = ctor.ParameterList.Parameters;

        var assignedNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var statement in ctor.Body!.Statements)
        {
            if (statement is not ExpressionStatementSyntax expressionStatement)
            {
                continue;
            }

            if (expressionStatement.Expression is not AssignmentExpressionSyntax assignment)
            {
                continue;
            }

            var leftName = assignment.Left switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier.Text,
                MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax, Name: { Identifier: var identifier } } => identifier.Text,
                _ => null,
            };

            if (leftName is not null)
            {
                _ = assignedNames.Add(leftName);
            }
        }

        var parameterNames = new HashSet<string>(
            parameters.Select(static p => p.Identifier.Text),
            StringComparer.Ordinal);

        var newMembers = new List<MemberDeclarationSyntax>();
        var fieldRenames = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var member in classDeclaration.Members)
        {
            if (member == ctor)
            {
                continue;
            }

            if (member is FieldDeclarationSyntax field
                && field.Declaration.Variables.Count == 1
                && field.Declaration.Variables[0].Initializer is null)
            {
                var match = MatchFieldToParam(
                    field.Declaration.Variables[0].Identifier.Text,
                    parameterNames);

                if (match.ParamName is not null)
                {
                    if (match.ParamName != match.FieldName)
                    {
                        fieldRenames[match.FieldName] = match.ParamName;
                    }
                    continue;
                }
            }

            newMembers.Add(member);
        }

        if (fieldRenames.Count > 0)
        {
            var rewriter = new FieldReferenceRewriter(fieldRenames);
            newMembers = newMembers
                .Select(m => m is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax
                    ? m
                    : (MemberDeclarationSyntax)(rewriter.Visit(m) ?? m))
                .ToList();
        }

        var newParameterList = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters));

        var newClass = classDeclaration
            .WithIdentifier(classDeclaration.Identifier.WithTrailingTrivia())
            .WithParameterList(newParameterList.WithLeadingTrivia().WithTrailingTrivia())
            .WithMembers(SyntaxFactory.List(newMembers));

        var newRoot = root.ReplaceNode(classDeclaration, newClass);
        return document.WithSyntaxRoot(newRoot);
    }

    private static (string? ParamName, string FieldName) MatchFieldToParam(
        string fieldName,
        HashSet<string> parameterNames)
    {
        if (parameterNames.Contains(fieldName))
        {
            return (fieldName, fieldName);
        }

        if (parameterNames.Contains("_" + fieldName))
        {
            return ("_" + fieldName, fieldName);
        }

        var trimmed = fieldName.TrimStart('_');
        if (trimmed.Length != fieldName.Length && parameterNames.Contains(trimmed))
        {
            return (trimmed, fieldName);
        }

        return (null, fieldName);
    }

    private sealed class FieldReferenceRewriter(IReadOnlyDictionary<string, string> fieldToParam) : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            return fieldToParam.TryGetValue(node.Identifier.Text, out var newName)
                ? node.WithIdentifier(
                    SyntaxFactory.Identifier(newName)
                        .WithLeadingTrivia(node.GetLeadingTrivia())
                        .WithTrailingTrivia(node.GetTrailingTrivia()))
                // boundary: base.VisitIdentifierName returns SyntaxNode? but the override is
                // declared SyntaxNode; for an IdentifierNameSyntax with no rename, the rewriter
                // returns the same node unchanged, which is never null.
                : base.VisitIdentifierName(node)!;
        }
    }
}
