# UnifiedOdds Feed SDK .NET library (.NET Standard 2.0)

Notice: before starting DemoProject make sure to enter your bookmaker access token in app.config file and restore nuget packages by right-clicking the solution item and selecting "Restore NuGet
Packages".

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
