using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.DotNet.UpgradeAssistant;
using System.Collections.Immutable;

namespace UpgradeAssistant.Extension.Maui.Community;

[ApplicableComponents(ProjectComponents.Maui)]
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UsingCommunityAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "COMUA0001";
    private const string Category = "Upgrade";
    private static readonly string[] DisallowedNamespaces = new[]
    {
        "Rg.Plugins.Popup",
        "Rg.Plugins.Popup.Services",
        "Rg.Plugins.Popup.Pages"
    };

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Remove Xamarin.Forms community packages namespace",
        "Namespace '{0}' should not be referenced in .NET MAUI projects",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "This using directive is not supported on .NET MAUI and should be replaced.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeUsingDirectives, SyntaxKind.UsingDirective);
        context.RegisterSyntaxNodeAction(AnalyzeQualifiedNames, SyntaxKind.QualifiedName);
    }

    private void AnalyzeUsingDirectives(SyntaxNodeAnalysisContext context)
    {
        var usingDirective = (UsingDirectiveSyntax)context.Node;

        var namespaceName = usingDirective.Name?.ToString();

        if (namespaceName is null)
        {
            return;
        }

        if (DisallowedNamespaces.Any(name => namespaceName.Equals(name, StringComparison.Ordinal) || namespaceName.StartsWith($"{name}.", StringComparison.Ordinal)))
        {
            var diagnostic = Diagnostic.Create(Rule, usingDirective.GetLocation(), namespaceName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private void AnalyzeQualifiedNames(SyntaxNodeAnalysisContext context)
    {
        var qualifiedNameNode = (QualifiedNameSyntax)context.Node;
        if (qualifiedNameNode is null)
        {
            return;
        }

        var parentNode = qualifiedNameNode.Parent;
        while (parentNode is not null)
        {
            if (parentNode.IsKind(SyntaxKind.UsingDirective) || parentNode.IsKind(SyntaxKind.UsingStatement))
            {
                return;
            }

            parentNode = parentNode.Parent;
        }

        var qualifiedName = qualifiedNameNode.ToString();
        if (DisallowedNamespaces.Any(name => qualifiedName.Equals(name, StringComparison.Ordinal)))
        {
            var diagnostic = Diagnostic.Create(Rule, qualifiedNameNode.GetLocation(), qualifiedName);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
