using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.DotNet.UpgradeAssistant;
using System.Collections.Immutable;

namespace UpgradeAssistant.Extension.Maui.Community;

[ApplicableComponents(ProjectComponents.Maui)]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = "Using Community Packages code fixer")]
public class UsingCommunityAnalyzerCodeFixProvider : CodeFixProvider
{
    private static readonly string[] NewCommunityNamespaces = new[]
    {
        "Mopups.Hosting",
        "Mopups.Pages",
        "Mopups.Services",
        "Mopups.Events"
    };

    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UsingCommunityAnalyzer.DiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider()
    {
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root is null)
        {
            return;
        }

        var node = root.FindNode(context.Span);

        if (node is null)
        {
            return;
        }

        // Register the appropriate code action that will invoke the fix
        switch (node.RawKind)
        {
            case (int)SyntaxKind.UsingDirective:
            case (int)SyntaxKind.UsingStatement:
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Remove Xamarin.Forms community packages namespace",
                        cancellationToken => ReplaceUsingStatementAsync(context.Document, node, cancellationToken)),
                    context.Diagnostics);
                break;
            case (int)SyntaxKind.QualifiedName:
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Remove Xamarin.Forms community packages namespace",
                        cancellationToken => RemoveNamespaceQualifierAsync(context.Document, node, cancellationToken)),
                    context.Diagnostics);
                break;
        }
    }

    private static async Task<Document> ReplaceUsingStatementAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        var documentRoot = (CompilationUnitSyntax)editor.OriginalRoot;
        documentRoot = documentRoot.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);

        foreach (var name in NewCommunityNamespaces)
        {
            documentRoot = documentRoot?.AddUsingIfMissing(name);
        }


        if (documentRoot is not null)
        {
            editor.ReplaceNode(editor.OriginalRoot, documentRoot);
        }

        return editor.GetChangedDocument();
    }

    private static async Task<Document> RemoveNamespaceQualifierAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        if (node.Parent is not null)
        {
            editor.ReplaceNode(node.Parent, node.Parent.ChildNodes().Last());
        }

        return editor.GetChangedDocument();
    }
}
