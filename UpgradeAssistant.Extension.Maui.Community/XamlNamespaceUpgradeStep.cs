using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.UpgradeAssistant;
using Microsoft.Extensions.Logging;

namespace UpgradeAssistant.Extension.Maui.Community;

public class XamlNamespaceUpgradeStep : UpgradeStep
{
    private static readonly IReadOnlyDictionary<string, string> XamarinToMauiReplacementMap = new Dictionary<string, string>
    {
        { "http://rotorgames.com", "clr-namespace:Mopups.Pages;assembly=Mopups" },
        { "clr-namespace:Rg.Plugins.Popup.Pages;assembly=Rg.Plugins.Popup", "clr-namespace:Mopups.Pages;assembly=Mopups"},
        { "clr-namespace:Rg.Plugins.Popup.Animations;assembly=Rg.Plugins.Popup", "clr-namespace:Mopups.Animations;assembly=Mopups" },
    };

    private readonly IPackageRestorer _restorer;

    public override string Title => "Update XAML Namespaces";

    public override string Description => "Updates XAML namespaces to .NET MAUI";

    public XamlNamespaceUpgradeStep(IPackageRestorer restorer, ILogger<XamlNamespaceUpgradeStep> logger)
        : base(logger)
    {
        _restorer = restorer;
    }

    public override IEnumerable<string> DependsOn { get; } = new[]
    {
        WellKnownStepIds.TemplateInserterStepId,
    };

    public override IEnumerable<string> DependencyOf { get; } = new[]
    {
        WellKnownStepIds.NextProjectStepId,
    };

    protected override async Task<UpgradeStepApplyResult> ApplyImplAsync(IUpgradeContext context, CancellationToken token)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var project = context.CurrentProject.Required();
        var roslynProject = GetBestRoslynProject(project.GetRoslynProject());
        var solution = roslynProject.Solution;

        foreach (var file in GetXamlDocuments(roslynProject))
        {
            var sourceText = await file.GetTextAsync(token).ConfigureAwait(false);
            var text = sourceText.ToString();

            // Make replacements...
            foreach (var key in XamarinToMauiReplacementMap.Keys)
            {
                text = text.Replace(key, XamarinToMauiReplacementMap[key]);
            }

            var newText = SourceText.From(text, encoding: sourceText.Encoding);

            solution = solution.WithAdditionalDocumentText(file.Id, newText);
        }

        var status = context.UpdateSolution(solution) ? UpgradeStepStatus.Complete : UpgradeStepStatus.Failed;

        return context.CreateAndAddStepApplyResult(this, status, "Updated XAML namespaces to .NET MAUI");
    }

    protected override async Task<UpgradeStepInitializeResult> InitializeImplAsync(IUpgradeContext context, CancellationToken token)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return await Task.Run(() =>
        {
            // With updated TFMs and UseMaui, we need to restore packages
            var project = context.CurrentProject.Required();
            var roslynProject = GetBestRoslynProject(project.GetRoslynProject());
            var hasXamlFiles = GetXamlDocuments(roslynProject).Any();
            if (hasXamlFiles)
            {
                Logger.LogInformation(".NET MAUI project has XAML files that may need to be updated");
                return new UpgradeStepInitializeResult(UpgradeStepStatus.Incomplete, ".NET MAUI project has XAML files that may need to be updated", BuildBreakRisk.High);
            }
            else
            {
                Logger.LogInformation(".NET MAUI project does not contain any XAML files");
                return new UpgradeStepInitializeResult(UpgradeStepStatus.Complete, ".NET MAUI project does not contain any XAML files", BuildBreakRisk.None);
            }
        }).ConfigureAwait(false);
    }

    protected override async Task<bool> IsApplicableImplAsync(IUpgradeContext context, CancellationToken token)
    {
        if (context is null)
        {
            return false;
        }

        if (context.CurrentProject is null)
        {
            return false;
        }

        var project = context.CurrentProject.Required();
        var components = await project.GetComponentsAsync(token).ConfigureAwait(false);
        if (components.HasFlag(ProjectComponents.MauiAndroid) || components.HasFlag(ProjectComponents.MauiiOS) || components.HasFlag(ProjectComponents.Maui))
        {
            return true;
        }

        return false;
    }

    private static IEnumerable<TextDocument> GetXamlDocuments(Project project)
        => project.AdditionalDocuments.Where(d => d.FilePath?.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase) == true);

    private static Project GetBestRoslynProject(Project project)
        => project.Solution.Projects
            .Where(p => p.FilePath == project.FilePath)
            .OrderByDescending(p => p.AdditionalDocumentIds.Count)
            .First();
}
