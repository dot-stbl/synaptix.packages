using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Synaptix.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Synaptix0003NoPrivateMethodCodeFixProvider)), Shared]
public sealed class Synaptix0003NoPrivateMethodCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(Synaptix0003NoPrivateMethodAnalyzer.DiagnosticId);

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

            var methodDecl = node as MethodDeclarationSyntax
                ?? node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (methodDecl is null)
            {
                continue;
            }

            // Only fix private static methods. Instance methods may depend on `this`
            // state and cannot be safely extracted without further analysis.
            if (!methodDecl.Modifiers.Any(SyntaxKind.PrivateKeyword))
            {
                continue;
            }

            if (!methodDecl.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                continue;
            }

            if (HasNonCopyableAttribute(methodDecl))
            {
                continue;
            }

            var classDecl = methodDecl.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDecl is null)
            {
                continue;
            }

            var helperClassName = classDecl.Identifier.Text + "Helpers";
            if (HasHelperClassCollision(classDecl, helperClassName))
            {
                continue;
            }

            // Skip when the class has overloads of this method. Moving the method
            // out of the class would change unqualified-call resolution, which can
            // become ambiguous. Caller can rename first, then extract.
            if (HasOverloads(classDecl, methodDecl))
            {
                continue;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Extract to file-scoped static class",
                    createChangedDocument: ct => ExtractAsync(
                        context.Document,
                        classDecl,
                        methodDecl,
                        helperClassName,
                        ct),
                    equivalenceKey: "STBL0003_ExtractToFileScopedStaticClass"),
                diagnostic);
        }

        await Task.CompletedTask;
    }

    private static bool HasNonCopyableAttribute(MethodDeclarationSyntax method)
    {
        // v1.0: only attributes with no method-level semantics beyond `private`
        // are considered copy-safe. [MethodImpl] is excluded because it is
        // typically used to opt the *containing type's* call sites into specific
        // JIT strategies; moving the method out of the type changes that surface.
        foreach (var attrList in method.AttributeLists)
        {
            foreach (var attr in attrList.Attributes)
            {
                var name = attr.Name.ToString();
                if (name.EndsWith("MethodImpl", StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasHelperClassCollision(ClassDeclarationSyntax classDecl, string helperClassName)
    {
        var parent = classDecl.Parent;
        if (parent is null)
        {
            return false;
        }

        foreach (var sibling in parent.ChildNodes())
        {
            if (sibling is ClassDeclarationSyntax other && other.Identifier.Text == helperClassName)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasOverloads(ClassDeclarationSyntax classDecl, MethodDeclarationSyntax methodDecl)
    {
        var name = methodDecl.Identifier.Text;
        foreach (var member in classDecl.Members.OfType<MethodDeclarationSyntax>())
        {
            if (member == methodDecl)
            {
                continue;
            }

            if (member.Identifier.Text == name)
            {
                return true;
            }
        }

        return false;
    }

    private static async Task<Document> ExtractAsync(
        Document document,
        ClassDeclarationSyntax classDecl,
        MethodDeclarationSyntax methodDecl,
        string helperClassName,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
        {
            return document;
        }

        var methodName = methodDecl.Identifier.Text;
        var movedMethodSymbol = semanticModel.GetDeclaredSymbol(methodDecl, cancellationToken);
        if (movedMethodSymbol is null)
        {
            return document;
        }

        // Identify the call sites that resolve to the moved method BEFORE we mutate
        // the tree — the semantic model is bound to the original root, so nodes
        // we discover later (after TrackNodes/ReplaceNode) can't be resolved
        // against it.
        var callSites = CollectCallSitesToQualify(
            root,
            methodName,
            helperClassName,
            movedMethodSymbol,
            semanticModel);

        // Track the original class, method, and every call site we plan to
        // rewrite so we can find their current versions after each
        // transformation. Without this, the references passed in here become
        // stale as soon as `ReplaceNode` rebuilds the parent chain, and the
        // moved method silently stays in place.
        var nodesToTrack = new List<SyntaxNode>(callSites.Count + 2) { classDecl, methodDecl };
        foreach (var (original, _) in callSites)
        {
            nodesToTrack.Add(original);
        }

        var workingRoot = root.TrackNodes(nodesToTrack);

        // 1. Qualify unqualified call sites. Qualified calls (e.g. `Foo.Bar()`)
        //    are left untouched — they typically resolve to public/internal
        //    APIs and need manual handling if they relied on private
        //    visibility.
        foreach (var (original, replacement) in callSites)
        {
            var currentInvocation = workingRoot.GetCurrentNode(original);
            if (currentInvocation is null)
            {
                continue;
            }

            workingRoot = workingRoot.ReplaceNode(currentInvocation, replacement);
        }

        // Re-resolve the class/method references after the call-site updates.
        var currentClassDecl = workingRoot.GetCurrentNode(classDecl)!;
        var currentMethodDecl = workingRoot.GetCurrentNode(methodDecl)!;

        // 2. Build the public-static version of the method. WithModifiers
        //    preserves the body, parameters, return type, attributes, XML docs,
        //    and trivia of the original.
        var helperMethod = currentMethodDecl.WithModifiers(
            SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword).WithTrailingTrivia(SyntaxFactory.Space),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword).WithTrailingTrivia(SyntaxFactory.Space)));

        // 3. Build the file-scoped static class containing the moved method.
        var helperClass = SyntaxFactory.ClassDeclaration(helperClassName)
            .WithModifiers(SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.FileKeyword).WithTrailingTrivia(SyntaxFactory.Space),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword).WithTrailingTrivia(SyntaxFactory.Space)))
            .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(helperMethod))
            .WithLeadingTrivia(SyntaxFactory.CarriageReturnLineFeed);

        // 4. Remove the method from the original class declaration and insert
        //    the helper class as the next sibling.
        var newRoot = MoveMethod(workingRoot, currentClassDecl, currentMethodDecl, helperClass);

        return document.WithSyntaxRoot(newRoot);
    }

    private static List<(InvocationExpressionSyntax Original, InvocationExpressionSyntax Replacement)> CollectCallSitesToQualify(
        SyntaxNode root,
        string methodName,
        string helperClassName,
        IMethodSymbol targetSymbol,
        SemanticModel semanticModel)
    {
        var result = new List<(InvocationExpressionSyntax, InvocationExpressionSyntax)>();

        foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (inv.Expression is MemberAccessExpressionSyntax)
            {
                continue;
            }

            if (inv.Expression is not IdentifierNameSyntax id)
            {
                continue;
            }

            if (id.Identifier.Text != methodName)
            {
                continue;
            }

            var symbolInfo = semanticModel.GetSymbolInfo(inv);
            if (symbolInfo.Symbol is null)
            {
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(symbolInfo.Symbol, targetSymbol))
            {
                continue;
            }

            var qualifiedAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(helperClassName),
                SyntaxFactory.IdentifierName(id.Identifier.Text))
                .WithTriviaFrom(id);

            var replacement = inv.WithExpression(qualifiedAccess);
            result.Add((inv, replacement));
        }

        return result;
    }

    private static SyntaxNode MoveMethod(
        SyntaxNode root,
        ClassDeclarationSyntax classDecl,
        MethodDeclarationSyntax methodDecl,
        ClassDeclarationSyntax helperClass)
    {
        var newClassMembers = new List<MemberDeclarationSyntax>(classDecl.Members.Count - 1);
        foreach (var member in classDecl.Members)
        {
            if (member != methodDecl)
            {
                newClassMembers.Add(member);
            }
        }

        var newClassDecl = classDecl.WithMembers(SyntaxFactory.List(newClassMembers));

        switch (classDecl.Parent)
        {
            case FileScopedNamespaceDeclarationSyntax fns:
                {
                    var index = fns.Members.IndexOf(classDecl);
                    var newMembers = fns.Members
                        .Replace(classDecl, newClassDecl)
                        .Insert(index + 1, helperClass);
                    return root.ReplaceNode(fns, fns.WithMembers(newMembers));
                }

            case NamespaceDeclarationSyntax ns:
                {
                    var index = ns.Members.IndexOf(classDecl);
                    var newMembers = ns.Members
                        .Replace(classDecl, newClassDecl)
                        .Insert(index + 1, helperClass);
                    return root.ReplaceNode(ns, ns.WithMembers(newMembers));
                }

            case CompilationUnitSyntax cu:
                {
                    var index = cu.Members.IndexOf(classDecl);
                    var newMembers = cu.Members
                        .Replace(classDecl, newClassDecl)
                        .Insert(index + 1, helperClass);
                    return root.ReplaceNode(cu, cu.WithMembers(newMembers));
                }

            case ClassDeclarationSyntax outerClass:
                {
                    var index = outerClass.Members.IndexOf(classDecl);
                    var newMembers = outerClass.Members
                        .Replace(classDecl, newClassDecl)
                        .Insert(index + 1, helperClass);
                    return root.ReplaceNode(outerClass, outerClass.WithMembers(newMembers));
                }

            case StructDeclarationSyntax outerStruct:
                {
                    var index = outerStruct.Members.IndexOf(classDecl);
                    var newMembers = outerStruct.Members
                        .Replace(classDecl, newClassDecl)
                        .Insert(index + 1, helperClass);
                    return root.ReplaceNode(outerStruct, outerStruct.WithMembers(newMembers));
                }

            case RecordDeclarationSyntax outerRecord:
                {
                    var index = outerRecord.Members.IndexOf(classDecl);
                    var newMembers = outerRecord.Members
                        .Replace(classDecl, newClassDecl)
                        .Insert(index + 1, helperClass);
                    return root.ReplaceNode(outerRecord, outerRecord.WithMembers(newMembers));
                }

            default:
                return root;
        }
    }
}