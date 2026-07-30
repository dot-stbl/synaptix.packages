using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Synaptix.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Synaptix0006TraditionalConstructorAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "STBL0006";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "STBL0006: Traditional constructor can be replaced with primary constructor",
        messageFormat: "Class '{0}' has a traditional constructor that can be replaced with a primary constructor (C# 12+)",
        category: AnalysisConstants.DiagnosticCategoryStyle,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A class with a single constructor whose body only assigns fields from parameters can be rewritten using a C# 12 primary constructor. Primary constructors eliminate the boilerplate of explicit field declarations and constructor assignments.",
        helpLinkUri: AnalysisConstants.HelpLinkBase + "STBL0006-traditional-constructor.md");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static compilationContext =>
        {
            if (compilationContext.Compilation is not CSharpCompilation csharpCompilation)
            {
                return;
            }

            var langVersion = csharpCompilation.LanguageVersion;
            if (langVersion is not (LanguageVersion.Default or >= LanguageVersion.CSharp12))
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                Analyze,
                SyntaxKind.ClassDeclaration);
        });
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclaration)
        {
            return;
        }

        if (classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword))
        {
            return;
        }

        if (classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return;
        }

        if (classDeclaration.ParameterList is not null)
        {
            return;
        }

        var ctors = classDeclaration.Members
            .OfType<ConstructorDeclarationSyntax>()
            .ToList();

        if (ctors.Count != 1)
        {
            return;
        }

        var ctor = ctors[0];

        if (ctor.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return;
        }

        if (ctor.AttributeLists.Any())
        {
            return;
        }

        if (ctor.Body is null)
        {
            return;
        }

        var statements = ctor.Body.Statements;
        if (!statements.Any())
        {
            return;
        }

        var parameterNames = new HashSet<string>(
            ctor.ParameterList.Parameters.Select(static p => p.Identifier.Text),
            StringComparer.Ordinal);

        foreach (var statement in statements)
        {
            if (statement is not ExpressionStatementSyntax expressionStatement)
            {
                return;
            }

            if (expressionStatement.Expression is not AssignmentExpressionSyntax assignment)
            {
                return;
            }

            if (assignment.Kind() != SyntaxKind.SimpleAssignmentExpression)
            {
                return;
            }

            if (assignment.Left is not IdentifierNameSyntax)
            {
                return;
            }

            if (assignment.Right is not IdentifierNameSyntax right)
            {
                return;
            }

            if (!parameterNames.Contains(right.Identifier.Text))
            {
                return;
            }
        }

        var location = Location.Create(
            classDeclaration.SyntaxTree,
            classDeclaration.Identifier.Span);

        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            location,
            classDeclaration.Identifier.Text));
    }
}
