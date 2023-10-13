# UpgradeAssistant.Extension.Maui.Community
A extension to help Xamarin to MAUI migration

## Upgrade assistante usage for MAUI


Install the [upgrade assistant CLI](https://learn.microsoft.com/en-us/dotnet/core/porting/upgrade-assistant-install-legacy#install-the-legacy-version)

`dotnet tool install upgrade-assistant -g --ignore-failed-sources --version 0.4.421302`

Go to project repository and add this extension

`upgrade-assistant extensions add UpgradeAssistant.Extension.Maui.Community --version 1.0.7`

Or add a `upgrade-assistant.json` file to your project repository path

```json
{
  "Extensions": [
    {
      "Name": "UpgradeAssistant.Extension.Maui.Community",
      "Version": "1.0.7",
      "Source": "https://api.nuget.org/v3/index.json"
    },
    {
      "Name": "Microsoft.DotNet.UpgradeAssistant.Extensions.Maui",
      "Version": "0.4.421302",
      "Source": "https://api.nuget.org/v3/index.json"
    }
  ]
}
```

Restore all extensions

`upgrade-assistant extensions restore`

Migrate your xamarin project to MAUI

`upgrade-assistant upgrade {yourprojectname}.csproj --ignore-unsupported-features` 

## Package map

|                             Xamarin Package                               |                               Maui Package                                                         |
|:-------------------------------------------------------------------------:|:--------------------------------------------------------------------------------------------------:|
|[Rg.Plugins.Popup](https://github.com/rotorgames/Rg.Plugins.Popup)         |[Mopups](https://github.com/LuckyDucko/Mopups)                                                      |
|[FFImageLoading](https://github.com/luberda-molinet/FFImageLoading)        |[FFImageLoadingCompat](https://github.com/Redth/FFImageLoading.Compat)                              |
|[PancakeView](https://github.com/sthewissen/Xamarin.Forms.PancakeView/)    |[Maui.PancakeView](https://github.com/felipebaltazar/Maui.PancakeView)                              |
|[Sharpnado.Shadows](https://github.com/roubachof/Sharpnado.Shadows)     |[SharpnadoCompat.Shadows](https://github.com/felipebaltazar/SharpnadoCompat.Shadows)                |
|[Xam.Plugin.LatestVersion](https://github.com/edsnider/LatestVersionPlugin)|[Maui.Plugin.LatestVersionCompat](https://github.com/felipebaltazar/Maui.Plugin.LatestVersionCompat)|


## Contributing

Just add Xamarin package map maui equivalent to [XamarinPackageMap.json](https://github.com/felipebaltazar/UpgradeAssistant.Extension.Maui.Community/blob/main/UpgradeAssistant.Extension.Maui.Community/PackageMaps/XamarinPackageMap.json)
and any Xaml namespace change to [XamlNamespaceUpgradeStep](https://github.com/felipebaltazar/UpgradeAssistant.Extension.Maui.Community/blob/main/UpgradeAssistant.Extension.Maui.Community/XamlNamespaceUpgradeStep.cs)
