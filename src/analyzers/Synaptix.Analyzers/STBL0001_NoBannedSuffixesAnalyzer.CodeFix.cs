using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace Synaptix.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Synaptix0001NoBannedSuffixesCodeFixProvider)), Shared]
public sealed class Synaptix0001NoBannedSuffixesCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(Synaptix0001NoBannedSuffixesAnalyzer.DiagnosticId);

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

        if (node is not TypeDeclarationSyntax typeDeclaration)
        {
            return;
        }

        var name = typeDeclaration.Identifier.Text;
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        var bannedSuffix = Synaptix0001NoBannedSuffixesAnalyzer.BannedSuffixesPublic
            .FirstOrDefault(s => name.Length > s.Length && name.EndsWith(s, StringComparison.Ordinal))!;

        if (bannedSuffix is null)
        {
            return;
        }

        var newName = name.Substring(0, name.Length - bannedSuffix.Length);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Strip banned suffix",
                createChangedSolution: ct => StripSuffixAsync(context.Document, typeDeclaration, newName, ct),
                equivalenceKey: "STBL0001_StripBannedSuffix"),
            diagnostic);

        await Task.CompletedTask;
    }

    private static async Task<Solution> StripSuffixAsync(
        Document document,
        TypeDeclarationSyntax typeDeclaration,
        string newName,
        CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
        {
            return document.Project.Solution;
        }

        var symbol = semanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken);
        if (symbol is null)
        {
            return document.Project.Solution;
        }

        var solution = document.Project.Solution;

        return await Renamer.RenameSymbolAsync(
            solution,
            symbol,
            new SymbolRenameOptions(),
            newName,
            cancellationToken).ConfigureAwait(false);
    }
}
