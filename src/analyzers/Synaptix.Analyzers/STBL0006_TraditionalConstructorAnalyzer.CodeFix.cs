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

            if (assignment.Left is IdentifierNameSyntax left)
            {
                _ = assignedNames.Add(left.Identifier.Text);
            }
        }

        var parameterNames = new HashSet<string>(
            parameters.Select(static p => p.Identifier.Text),
            StringComparer.Ordinal);

        var newMembers = new List<MemberDeclarationSyntax>();
        foreach (var member in classDeclaration.Members)
        {
            if (member == ctor)
            {
                continue;
            }

            if (member is FieldDeclarationSyntax field
                && ShouldRemoveField(field, assignedNames, parameterNames))
            {
                continue;
            }

            newMembers.Add(member);
        }

        var newParameterList = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters));

        var newClass = classDeclaration
            .WithIdentifier(classDeclaration.Identifier.WithTrailingTrivia())
            .WithParameterList(newParameterList.WithLeadingTrivia().WithTrailingTrivia())
            .WithMembers(SyntaxFactory.List(newMembers));

        var newRoot = root.ReplaceNode(classDeclaration, newClass);
        return document.WithSyntaxRoot(newRoot);
    }

    private static bool ShouldRemoveField(
        FieldDeclarationSyntax field,
        HashSet<string> assignedNames,
        HashSet<string> parameterNames)
    {
        var variables = field.Declaration.Variables;
        if (variables.Count != 1)
        {
            return false;
        }

        var variable = variables[0];

        if (variable.Initializer is not null)
        {
            return false;
        }

        var fieldName = variable.Identifier.Text;

        return assignedNames.Contains(fieldName) && parameterNames.Contains(fieldName);
    }
}
