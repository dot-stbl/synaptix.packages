using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Synaptix.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Synaptix0007NoUnderscoreFieldPrefixAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "STBL0007";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "STBL0007: Private field uses banned underscore prefix",
        messageFormat: "Private field '{0}' uses banned underscore prefix. Use camelCase without leading underscore.",
        category: AnalysisConstants.DiagnosticCategoryNaming,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Private fields must not use a leading underscore. The .stbl convention is to expose them through primary constructors (where the parameter is the field), or to use a plain camelCase name without an underscore prefix.",
        helpLinkUri: AnalysisConstants.HelpLinkBase + "STBL0007-no-underscore-field-prefix.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(
            Analyze,
            SyntaxKind.FieldDeclaration);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not FieldDeclarationSyntax fieldDeclaration)
        {
            return;
        }

        if (!fieldDeclaration.Modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            return;
        }

        if (fieldDeclaration.AttributeLists.Count > 0)
        {
            foreach (var attributeList in fieldDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var name = attribute.Name.ToString();
                    if (name == "SuppressMessage" || name.EndsWith(".SuppressMessage", StringComparison.Ordinal))
                    {
                        return;
                    }
                }
            }
        }

        foreach (var variable in fieldDeclaration.Declaration.Variables)
        {
            var identifier = variable.Identifier;
            var name = identifier.Text;

            if (string.IsNullOrEmpty(name) || name[0] != '_')
            {
                continue;
            }

            var location = Location.Create(fieldDeclaration.SyntaxTree, identifier.Span);
            context.ReportDiagnostic(Diagnostic.Create(
                Rule,
                location,
                name));
        }
    }
}
