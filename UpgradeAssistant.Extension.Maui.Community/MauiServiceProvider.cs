using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.DotNet.UpgradeAssistant;
using Microsoft.DotNet.UpgradeAssistant.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace UpgradeAssistant.Extension.Maui.Community;

public class MauiServiceProvider : IExtensionServiceProvider
{
    public void AddServices(IExtensionServiceCollection services)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.Services.AddUpgradeStep<XamlNamespaceUpgradeStep>();
        services.Services.AddTransient<DiagnosticAnalyzer, UsingCommunityAnalyzer>();
        services.Services.AddTransient<CodeFixProvider, UsingCommunityAnalyzerCodeFixProvider>();
    }
}
