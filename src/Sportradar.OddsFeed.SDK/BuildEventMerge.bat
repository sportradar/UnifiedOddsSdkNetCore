set bat_dir=%~dp0

echo Merging dlls ...

mkdir "..\..\lib\net45"
del "..\..\lib\net45\Sportradar.OddsFeed.SDK.dll"
del  "..\..\..\Sportradar.OddsFeed.SDK.DemoProject\resources\Sportradar.OddsFeed.SDK.dll"

pushd "%bat_dir%"
"C:\Program Files (x86)\Microsoft\ILMerge\ILMerge.exe" /internalize:"DoNotInternalize.txt" /ndebug /copyattrs /targetplatform:4.0,"C:\Windows\Microsoft.NET\Framework64\v4.0.30319"  /out:"..\..\lib\net45\Sportradar.OddsFeed.SDK.dll" "Sportradar.OddsFeed.SDK.dll" "Sportradar.OddsFeed.SDK.Common.dll" "Sportradar.OddsFeed.SDK.Entities.dll" "Sportradar.OddsFeed.SDK.Entities.REST.dll" "Sportradar.OddsFeed.SDK.Messages.dll" "RabbitMQ.Client.dll" "Microsoft.Practices.Unity.dll" "Microsoft.Practices.ServiceLocation.dll" "af\Humanizer.resources.dll" "ar\Humanizer.resources.dll" "bg\Humanizer.resources.dll" "bn-BD\Humanizer.resources.dll" "cs\Humanizer.resources.dll" "da\Humanizer.resources.dll" "de\Humanizer.resources.dll" "el\Humanizer.resources.dll" "es\Humanizer.resources.dll" "fa\Humanizer.resources.dll" "fi-FI\Humanizer.resources.dll" "fr-BE\Humanizer.resources.dll" "fr\Humanizer.resources.dll" "he\Humanizer.resources.dll" "hr\Humanizer.resources.dll" "hu\Humanizer.resources.dll" "id\Humanizer.resources.dll" "it\Humanizer.resources.dll" "ja\Humanizer.resources.dll" "nb-NO\Humanizer.resources.dll" "nb\Humanizer.resources.dll" "nl\Humanizer.resources.dll" "pl\Humanizer.resources.dll" "pt\Humanizer.resources.dll" "ro\Humanizer.resources.dll" "ru\Humanizer.resources.dll" "sk\Humanizer.resources.dll" "sl\Humanizer.resources.dll" "sr-Latn\Humanizer.resources.dll" "sr\Humanizer.resources.dll" "sv\Humanizer.resources.dll" "tr\Humanizer.resources.dll" "uk\Humanizer.resources.dll" "uz-Cyrl-UZ\Humanizer.resources.dll" "uz-Latn-UZ\Humanizer.resources.dll" "vi\Humanizer.resources.dll" "zh-CN\Humanizer.resources.dll" "zh-Hans\Humanizer.resources.dll" "zh-Hant\Humanizer.resources.dll" "Humanizer.dll" "Metrics.dll" "Sportradar.OddsFeed.SDK.API.dll"

@echo off
echo ILmerge successfully executed.

copy "..\..\lib\net45\Sportradar.OddsFeed.SDK.dll" "..\..\..\Sportradar.OddsFeed.SDK.DemoProject\resources\Sportradar.OddsFeed.SDK.dll"

echo Creating nuget package ...

..\..\..\..\tools\nuget.exe pack ..\..\NugetPackage.nuspec

echo.
echo NuGet package successfully created.
echo.
popd