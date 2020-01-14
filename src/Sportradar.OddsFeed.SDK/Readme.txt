UnifiedOdds Feed SDK .NET library (.NET Standard 2.1)

For more information please contact support@sportradar.com or visit https://iodocs.betradar.com/unifiedsdk/index.html

CHANGE LOG:
2020-01-14  1.1.0.0
Added new Replay API endpoints (FEEDSDK-1316)
Updated DemoProject to use ILoggerFactory
Added metrics to SpecificEntityWriter in DemoProject
Fix: fetching outcome mappings for special markets that exists only on dynamic variant endpoint (FEEDSDK-1314)

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
