UnifiedOdds Feed SDK .NET library

Notice: before starting DemoProject make sure to enter your bookmaker access token in app.config file and restore nuget packages by right-clicking the solution item and selecting "Restore NuGet Packages".

The SDK is also available via NuGet package manager. Use the following command in the Package Manager Console to install it along with it's dependencies:
    Install-Package Sportradar.OddsFeed.SDK
    
The SDK uses the following 3rd party libraries which must be added via the NuGet package manager
    - id="Common.Logging" version="3.4.1"
    - id="Castle.Core" version="3.3.2"
    - id="CommonServiceLocator" version="1.3.0"
    - id="Humanizer" version="2.4.2"
    - id="log4net" version="2.0.8"
    - id="RabbitMQ.Client" version="3.6.2"
    - id="Unity" version="4.0.1"
    - id="Metrics.NET" version="0.5.5"
    - id="Microsoft.CSharp" version="4.5.0"

The package contains:
    - DemoProject: A Visual Studio 2019 solution containing a demo project showing the basic usage of the SDK
    - libs: DLL file composing the Odds Feed SDK
    - Odds Feed SDK Documentation.chm: A documentation file describing exposed entities
    - Resources containing the log4net configuration needed by the Odds Feed SDK

For more information please contact support@sportradar.com or visit https://iodocs.betradar.com/unifiedsdk/index.html

CHANGE LOG:
2019-10-05  1.0.0.0
Exposed option to delete old matches from cache - introduced ISportDataProviderV4
Loading home and away penalty score from penalty PeriodScore if present
Fix: return types in ISportDataProviderV3 (breaking change)
Fix: updated CustomConfigurationBuilder not to override pre-configured values
Fix: OutcomeDefinition null for variant markets
Fix: ProfileCache - CommunicationException is not wrapped in CacheItemNotFoundException
Fix: schedule date between normal and virtual feed synchronized
Fix: SportDataProvider methods invokes API requests for correct language
