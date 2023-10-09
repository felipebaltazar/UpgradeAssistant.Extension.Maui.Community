using Microsoft.DotNet.UpgradeAssistant;
using Microsoft.DotNet.UpgradeAssistant.Extensions;

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
    }
}
