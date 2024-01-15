using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UpgradeAssistant.Extension.Maui.Community;

public static partial class CodeFixHelpers
{
    public static CompilationUnitSyntax AddUsingIfMissing(this CompilationUnitSyntax documentRoot, string namespaceName)
    {
        if (documentRoot is null)
        {
            throw new ArgumentNullException(nameof(documentRoot));
        }

        if (string.IsNullOrEmpty(namespaceName))
        {
            throw new ArgumentException($"'{nameof(namespaceName)}' cannot be null or empty.", nameof(namespaceName));
        }

        if (documentRoot.Usings.Any(u => u.Name.ToString().Equals(namespaceName, StringComparison.Ordinal)))
        {
            return documentRoot;
        }

        var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(namespaceName).WithLeadingTrivia(SyntaxFactory.Whitespace(" ")))
            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
        return documentRoot.AddUsings(usingDirective);
    }
}
