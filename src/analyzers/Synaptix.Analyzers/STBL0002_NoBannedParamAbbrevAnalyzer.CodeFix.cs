using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace Synaptix.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Synaptix0002NoBannedParamAbbrevCodeFixProvider)), Shared]
public sealed class Synaptix0002NoBannedParamAbbrevCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(Synaptix0002NoBannedParamAbbrevAnalyzer.DiagnosticId);

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

            var parameterSyntax = node as ParameterSyntax
                ?? node.FirstAncestorOrSelf<ParameterSyntax>();
            if (parameterSyntax is null)
            {
                continue;
            }

            var parameterSymbol = semanticModel.GetDeclaredSymbol(parameterSyntax, context.CancellationToken);
            if (parameterSymbol is null)
            {
                continue;
            }

            var oldName = parameterSymbol.Name;
            if (!Synaptix0002NoBannedParamAbbrevAnalyzer.ExpansionMap.TryGetValue(oldName, out var newName))
            {
                continue;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: $"Rename parameter to '{newName}'",
                    createChangedSolution: ct => RenameParameterAsync(
                        context.Document,
                        parameterSymbol,
                        newName,
                        ct),
                    equivalenceKey: $"STBL0002_RenameTo_{newName}"),
                diagnostic);
        }

        await Task.CompletedTask;
    }

    private static async Task<Solution> RenameParameterAsync(
        Document document,
        IParameterSymbol parameterSymbol,
        string newName,
        CancellationToken cancellationToken)
    {
        return await Renamer.RenameSymbolAsync(
            document.Project.Solution,
            parameterSymbol,
            new SymbolRenameOptions(),
            newName,
            cancellationToken).ConfigureAwait(false);
    }
}
