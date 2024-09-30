# UnifiedOdds Feed SDK .NET library (.NET Standard 2.0)


Notice: before starting DemoProject make sure to enter your bookmaker access token in app.config file and restore nuget packages by right-clicking the solution item and selecting "Restore NuGet Packages".

The SDK is also available via NuGet package manager. Use the following command in the Package Manager Console to install it along with its dependencies:

`Install-Package Sportradar.OddsFeed.SDKCore`
    
The SDK uses the following 3rd party libraries which must be added via the NuGet package manager
- OpenTelemetry (1.6.0)
- Dawn.Guard (1.12.0)
- Humanizer (2.14.1)
- Microsoft.Extensions.Logging (7.0.0)
- Microsoft.Extensions.Caching.Memory (7.0.0)
- Microsoft.Extensions.Configuration (7.0.0)
- Microsoft.Extensions.Diagnostics.HealthChecks (7.0.11)
- Microsoft.Extensions.Http (7.0.0)
- RabbitMQ.Client (6.5.0)

The package contains:
- DemoProject: A Visual Studio 2022 solution containing a demo project showing the basic usage of the SDK
- libs: DLL file composing the Odds Feed SDK
- Odds Feed SDK Documentation.chm: A documentation file describing exposed entities
- Resources containing the log4net configuration needed by the Odds Feed SDK

For more information please contact support@sportradar.com or visit https://iodocs.betradar.com/unifiedsdk/index.html

## Breaking changes when migrating from 1.32.0 to 2.0.0
More can be found in the github docs documentation.
- TargetFramework changed from netstandard2.1 to netstandard2.0
- Removed all default method implementations on interfaces
- Removed obsolete property or methods:
    - IOutcomeSettlement.Result
    - IOddsFeedConfigurationSection.UseIntegrationEnvironment
    - IRound.GroupName
    - IRound.GetGroupName()
- Root classes renamed
    - IOddsFeed to IUofSdk
    - Feed to UofSdk
    - ReplayFeed to UofSdkForReplay
    - IOddsFeedExt to IUofSdkExtended
    - FeedExt to UofSdkExtended
    - IOddsFeedConfigurationSection to IUofConfigurationSection
    - OddsFeedConfigurationSection to UofConfigurationSection
    - IOddsFeedConfiguration to IUofConfiguration
    - Introduced IUofAdditionalConfiguration
    - Removed OperationManager (properties moved to IUofConfiguration)
    - IEnvironmentSelector - removed SelectIntegration() and SelectProduction() - use SelectEnvironment(SdkEnvironment ufEnvironment)
    - Renamed Feed.CreateBuilder() to UofSdk.GetSessionBuilder()
    - App.config section moved to Sportradar.OddsFeed.SDK.Api.Internal.Config.UofConfigurationSection
    - UofConfigurationSection property supportedLanguages renamed to desiredLanguages
- Renamed IRound.Name to Names
- Renamed IRound.PhaseOrGroupLongName to PhaseOrGroupLongNames
- IProducerManager method Get() renamed to GetProducer()
- Renamed suffix DTO to Dto (internal)
- Removed CI suffix in Exportable classes (internal)
- Renamed suffix CI to CacheItem (internal)
- Enum values in MessageType changed to CamelCase
- Enum values in ExceptionHandlingStrategy changed to CamelCase
- Enum values in CashoutStatus changed to CamelCase
- Enum values in FixtureChangeType changed to CamelCase
- Enum values in MarketStatus changed to CamelCase
- Enum values in OddsChangeReason changed to CamelCase
- Enum values in PropertyUsage changed to CamelCase
- Enum values in ResourceTypeGroup changed to CamelCase
- Dependent library changes:
    - Removed Newtonsoft.Json
    - Humanizer 2.8.26 -> 2.14.1
    - RabbitMQ.Client 5.1.2 -> 6.5.0
    - Microsoft.Extensions.Logging.Abstractions 3.1.0 -> 7.0.0
    - System.Configuration.ConfigurationManager 4.7.0 -> 7.0.0 - Should be changed with Microsoft.Extensions.Configuration 7.0.0,
    - Removed System.Runtime.Caching 4.7.0 replaced with Microsoft.Extensions.Caching.Memory
    - App.Metrics replaced with OpenTelemetry 1.6.0
    - Unity replaced with Microsoft.Extensions.DependencyInjection 7.0.0
    - Introduced Microsoft.Extensions.Diagnostics.HealthChecks 7.0.11
    - Introduced Microsoft.Extensions.Http 7.0.0
    - Removed Castle.Core
- Tests projects upgraded to NET 6.0
- Tests projects depended libraries upgraded to latest versions
- Inconsistent naming
    - IOddsFeedConfigurationSection.UseSSL -> UseSsl
    - IOddsFeedConfigurationSection.UseApiSSL -> UseApiSsl
    - ReplayPlayerStatus.Setting_up - SettingUp
    - IFixture.StartTimeTBD -> StartTimeTbd
    - ExportableFixture.StartTimeTBD -> StartTimeTbd
    - EventStatus.Not_Started -> NotStarted
    - FeedMessage.EventURN -> EventUrn
    - URN -> Urn
- Changed namespace
    - API namespace renamed to Api
    - REST namespace renamed to Rest
    - replay interfaces to Api.Replay
    - feed managers to Api.Managers
    - feed providers to Api.Managers
    - enum types moved to Common.Enums
    - IOddsFeedConfigurationSection moved to Api.Internal.Config
    - configuration interfaces moved to Api.Config
    - MessageInterest class moved to Api.Config

change 
```xml
  <configSections>
    <section name="oddsFeedSection" type="Sportradar.OddsFeed.SDK.API.Internal.OddsFeedConfigurationSection, Sportradar.OddsFeed.SDK" />
  </configSections>
```
to
```xml
  <configSections>
    <section name="uofSdkSection" type="Sportradar.OddsFeed.SDK.Api.Internal.Config.UofConfigurationSection, Sportradar.OddsFeed.SDK" />
  </configSections>
```
