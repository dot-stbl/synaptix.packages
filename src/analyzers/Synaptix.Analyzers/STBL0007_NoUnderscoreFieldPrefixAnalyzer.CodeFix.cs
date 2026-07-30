using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace Synaptix.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Synaptix0007NoUnderscoreFieldPrefixCodeFixProvider)), Shared]
public sealed class Synaptix0007NoUnderscoreFieldPrefixCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(Synaptix0007NoUnderscoreFieldPrefixAnalyzer.DiagnosticId);

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

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var span = diagnostic.Location.SourceSpan;
            var node = root.FindNode(span, getInnermostNodeForTie: true);

            var variableDeclarator = node as VariableDeclaratorSyntax
                ?? node.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
            if (variableDeclarator is null)
            {
                continue;
            }

            if (semanticModel.GetDeclaredSymbol(variableDeclarator, context.CancellationToken) is not IFieldSymbol fieldSymbol)
            {
                continue;
            }

            var oldName = fieldSymbol.Name;
            if (string.IsNullOrEmpty(oldName) || oldName[0] != '_')
            {
                continue;
            }

            var newName = oldName.TrimStart('_');

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Drop underscore prefix",
                    createChangedSolution: ct => RenameFieldAsync(
                        context.Document,
                        fieldSymbol,
                        newName,
                        ct),
                    equivalenceKey: $"STBL0007_DropUnderscorePrefix_{newName}"),
                diagnostic);
        }

        await Task.CompletedTask;
    }

    private static async Task<Solution> RenameFieldAsync(
        Document document,
        IFieldSymbol fieldSymbol,
        string newName,
        CancellationToken cancellationToken)
    {
        return await Renamer.RenameSymbolAsync(
            document.Project.Solution,
            fieldSymbol,
            new SymbolRenameOptions(),
            newName,
            cancellationToken).ConfigureAwait(false);
    }
}
