using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Synaptix.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Synaptix0008NonSealedClassCodeFixProvider)), Shared]
public sealed class Synaptix0008NonSealedClassCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(Synaptix0008NonSealedClassAnalyzer.DiagnosticId);

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

            var classDeclaration = node as ClassDeclarationSyntax
                ?? node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration is null)
            {
                continue;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add sealed modifier",
                    createChangedDocument: ct => AddSealedAsync(context.Document, classDeclaration, ct),
                    equivalenceKey: "STBL0008_AddSealedModifier"),
                diagnostic);
        }

        await Task.CompletedTask;
    }

    private static async Task<Document> AddSealedAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var sealedToken = SyntaxFactory.Token(SyntaxKind.SealedKeyword);
        var newModifiers = classDeclaration.Modifiers.Add(sealedToken);

        var newClassDeclaration = classDeclaration.WithModifiers(newModifiers);
        var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

        return document.WithSyntaxRoot(newRoot);
    }
}
