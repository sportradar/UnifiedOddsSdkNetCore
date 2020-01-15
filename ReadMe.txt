UnifiedOdds Feed SDK .NET library (.NET Standard 2.1)

Notice: before starting DemoProject make sure to enter your bookmaker access token in app.config file and restore nuget packages by right-clicking the solution item and selecting "Restore NuGet Packages".

The SDK is also available via NuGet package manager. Use the following command in the Package Manager Console to install it along with it's dependencies:
    Install-Package Sportradar.OddsFeed.SDKCore
    
The SDK uses the following 3rd party libraries which must be added via the NuGet package manager
    - App.Metrics (3.2.0)
    - Castle.Core (4.4.0)
    - Dawn.Guard (1.10.0)
    - Humanizer (2.7.9)
    - Microsoft.Charp (4.7.0)
    - Microsoft.Extensions.Logging (3.1.0)
    - Newtonsoft.Json (12.0.3)
    - RabbitMQ.Client (4.7.0)
    - Unity (5.11.3)

The package contains:
    - DemoProject: A Visual Studio 2019 solution containing a demo project showing the basic usage of the SDK
    - libs: DLL file composing the Odds Feed SDK
    - Odds Feed SDK Documentation.chm: A documentation file describing exposed entities
    - Resources containing the log4net configuration needed by the Odds Feed SDK

For more information please contact support@sportradar.com or visit https://iodocs.betradar.com/unifiedsdk/index.html

CHANGE LOG:
2020-01-15  1.1.1.0
Fix: DI error for FeedRecoveryManager

2020-01-14  1.1.0.0
Added new Replay API endpoints
Updated DemoProject to use ILoggerFactory
Added metrics to SpecificEntityWriter in DemoProject
Fix: fetching outcome mappings for special markets that exists only on dynamic variant endpoint

2020-01-06  1.0.0.0
Port of UF SDK to .NET Standard 2.1
Replaced Metrics.NET with App.Metrics
Replaced Code.Contracts with Dawn.Guard conditions
Replaced Common.Logging with Microsoft.Extensions.Logging
Upgraded RabbitMQ.Client to v5.1.2
Upgraded Newtonsoft.Json to v12.0.3
Upgraded Unity to 5.11.3
Removed obsolete methods and properties
Merged V1, V2, ... extended interfaces into base one