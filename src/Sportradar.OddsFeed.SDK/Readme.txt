UnifiedOdds Feed SDK .NET library (.NET Standard 2.1)

For more information please contact support@sportradar.com or visit https://iodocs.betradar.com/unifiedsdk/index.html

CHANGE LOG:
2020-03-23  1.4.0.0
Fix: invalid timestamp for cashout probabilities
Fix: handle settlement markets without outcomes
Fix: EventRecoveryCompleted is properly raised when snapshot completes

2020-03-16  1.3.0.0
Fix: added IOutcomeSettlement.OutcomeResult instead of IOutcomeSettlement.Result (obsolete)
Fix: competitor references for seasons
Fix: failing API requests on some configurations

2020-02-18  1.2.0.0
Added State to the Competitor
Added State to the Venue
Extended ISportInfoProvider.DeleteSportEventFromCache with option to delete sport event status
Improved logging for connection errors
Improved fetching fixtures for Replay environment
Fix: calling variant endpoint only if user requests market data
Fix: NullPointerException in ReplayManager

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
