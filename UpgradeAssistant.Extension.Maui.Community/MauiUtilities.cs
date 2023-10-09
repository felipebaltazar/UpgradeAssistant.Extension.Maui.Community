using Microsoft.DotNet.UpgradeAssistant;

namespace UpgradeAssistant.Extension.Maui.Community;

internal static class MauiUtilities
{
    public static UpgradeStepApplyResult CreateAndAddStepApplyResult(this IUpgradeContext context, UpgradeStep step, UpgradeStepStatus status, string message, string? location = null, string? details = null)
    {
        context?.AddResultForStep(step, location ?? context.CurrentProject?.GetFile()?.FilePath ?? string.Empty, status, message, details);
        return new UpgradeStepApplyResult(status, string.IsNullOrEmpty(details) ? message : string.Concat(message, Environment.NewLine, details));
    }

    public static bool IsNetStandard(this IProject project)
    {
        return project?.TargetFrameworks.Any(x => x.IsNetStandard) ?? false;
    }

    public static void RuntimePropertyMapper(IProjectPropertyElements projectProperties, IProjectFile file, string oldRuntimePropertyName)
    {
        // following conversion mapping here : https://github.com/xamarin/xamarin-macios/wiki/Project-file-properties-dotnet-migration
        var runtimeMapping = new Dictionary<string, string>()
        {
            { "x86_64", "iossimulator-x64;" },
            { "i386", "iossimulator-x86;" },
            { "ARM64", "ios-arm64;" },
            { "x86_64+i386", "iossimulator-x86;iossimulator-x64;" },
            { "ARMv7+ARM64+i386", "ios-arm;ios-arm64;" },
        };

        var runtimeProps = projectProperties.GetProjectPropertyValue(oldRuntimePropertyName).Distinct();
        var runtimeIdentifierString = string.Empty;
        foreach (var prop in runtimeProps)
        {
            runtimeIdentifierString += runtimeMapping[prop];
        }

        // remove old properties before adding new
        projectProperties.RemoveProjectProperty(oldRuntimePropertyName);
        if (runtimeIdentifierString.Count(x => x.Equals(';')) > 1)
        {
            file.SetPropertyValue("RuntimeIdentifiers", runtimeIdentifierString);
        }
        else
        {
            file.SetPropertyValue("RuntimeIdentifier", runtimeIdentifierString);
        }
    }

    public static void TransformProperty(IProjectPropertyElements projectProperties, IProjectFile file, string oldPropertyName, string newPropertyName, string oldPropertyValue = "", string newPropertyValue = "")
    {
        var currentPropertyValue = projectProperties.GetProjectPropertyValue(oldPropertyName).FirstOrDefault();
        if (!string.IsNullOrEmpty(currentPropertyValue))
        {
            projectProperties.RemoveProjectProperty(oldPropertyName);

            if (string.Equals(currentPropertyValue, oldPropertyValue, StringComparison.OrdinalIgnoreCase))
            {
                file.SetPropertyValue(newPropertyName, newPropertyValue);
            }

            if (string.IsNullOrEmpty(newPropertyValue))
            {
                file.SetPropertyValue(newPropertyName, currentPropertyValue);
            }
            else
            {
                file.SetPropertyValue(newPropertyName, newPropertyValue);
            }
        }
    }
}
