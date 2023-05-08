# UnifiedOdds Feed SDK .NET library (.NET Standard 2.1)

Notice: before starting DemoProject make sure to enter your bookmaker access token in app.config file and restore nuget packages by right-clicking the solution item and selecting "Restore NuGet Packages".

The SDK is also available via NuGet package manager. Use the following command in the Package Manager Console to install it along with it's dependencies:

`Install-Package Sportradar.OddsFeed.SDKCore`
    
The SDK uses the following 3rd party libraries which must be added via the NuGet package manager
- App.Metrics (3.2.0)
- Castle.Core (4.4.0)
- Dawn.Guard (1.12.0)
- Humanizer (2.8.26)
- Microsoft.CSharp (4.7.0)
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

# Change Log

## 2023-04-27  1.31.0
- Introduced OperationManager.MaxConnectionsPerServer property (in DI HttpClientHandler is created with setting MaxConnectionsPerServer)
- Fixed lock issue when calling SportDataProvider.GetActiveTournamentsAsync repeatedly
- Improved performance for market/outcome name generation
- CustomBetManager respects configured ExceptionHandlingStrategy 
- BookingManager respects configured ExceptionHandlingStrategy
- Fixed WNS API endpoints (removed /sports/ from url)
- Optimized daily cache population of available sport and category data (does not delete any previous ones anymore)
- Optimized execution of some internal async  methods

## 2023-02-03  1.30.1
- Improved speed on API requests with high concurrency

## 2023-01-26  1.30.0
- IProducer extended with Scope property
- Added support for group urn type
- Changing API host for replay to non-global
- Optimizing number of competitor or player profile api calls
- Improvement so less chance the competition event returns null category
- Improvement so less chance the competition event returns null tournament
- Improved recovery retry on missing messages
- Fix ignore fixture_change_fixture endpoint for virtual producers
- Fix for competitor reference print

## 2022-12-05  1.29.1
- Fix configuration apiHost for replay environment
- Fix merging of new sport and category which is not included in All tournaments for all sports endpoint

## 2022-11-09  1.29.0
- Enabled BookingStatus for stages fetches fixture
- Fix for variant market descriptions (to ignore default static market description - effects playerprops markets)
- Fix for fetching missing summary only if required (effects ILongTermEvent entities)

## 2022-09-20  1.28.0
- CustomBet - added support for calculate-filter endpoint

## 2022-07-15  1.27.0
- Improved API data distribution to SportEventCache
- Setup that each API endpoint on critical path has its own HttpClient
- Fix: recovery request url when configuring custom environment

## 2022-06-02  1.26.0
- Improvements for saving api data within caches (single item and for list of items)
- Changed the timestamp for periodic deletion of old event from cache (to now-12h)
- Improved logging format in LogHttpDataFetcher and SportDataProvider
- Improved logging for exceptions and warnings
- Added Age to FeedMessage string
- Added isOpen to OddsFeed

## 2022-04-26  1.25.0
- Separate HttpClient for critical (summary, player profile, competitor profile and variant market description) and other requests
- Added configuration option for fast HttpClient in OperationManager (default timeout 5s)
- Extended ISportDataProvider with GetTimelineEventsAsync
- Improved merging of competitor profile
- Modified sliding expiration of profile cache items to avoid GC congestion
- Improved how SportDataProvider is handling exceptions
- Improved metrics and logging for raw data events
- Improved metrics with app and system metrics
- Added metrics for SemaphorePool
- Fixed exception handling in DataRouterManager
- Extended RawApiDataEventArgs with RequestParams, RequestTime and Language
- Other minor improvements for observability

## 2022-02-23  1.24.0
- Added BetradarName to IRound
- Fix: ICompetition competitors did not expose IsVirtual correctly

## 2021-12-10  1.23.0
- Added support for results in sportEventStatus received from api
- Added new log messages during recovery requests
- Improved how merging is done within Competitor
- Improved connection recovery after long disconnect
- Removed unnecessary locks in SportEventStatusCache
- Fix: connecting to replay server with production token
- Fix: some fields in raw feed message was not filled
- Fix: throws exception if match, stage or draw not found exception happens

## 2021-11-18  1.22.0
- Improvements for connection resilience
- Added event RecoveryInitiated to IOddsFeed
- Added RabitConnectionTimeout and RabbitHeartbeat to OperationManager
- Improved logging regarding connection and recovery process
- Changed default UF environment from Integration to GlobalIntegration
- Extended StageType with Run enum value
- Fix: how connection is made
- Fix: case when 2 rabbit connections are made
- Fix: getting the names of category for simple_tournaments

## 2021-10-06  1.21.0
- Extended configuration with ufEnvironment attribute
- Extended ITokenSetter and IEnvironmentSelector 
- New values added to SdkEnvironment enum (GlobalIntegration, GlobalProduction, ProxySingapore, ProxyTokyo)

## 2021-09-09  1.20.0
- Improved exporting/importing and merging of player profile data
- Improved SemaphorePool handling
- Improvement: when fetching non-cached fixture change endpoint fails due to server error, try also normal fixture endpoint
- Improved tracking of last message timestamp per producer
- Fix: DI for SportEventStatusCache
- Fix: wrong max recovery time was used (now default is 1h)
- Fix: issue with concurrency when getting missing languages for competitor profile
- Fix: merging market mapping when no outcome mappings exists
- Fix: how after parameter is checked when adjustAfterAge in config is set
- Other minor improvements and bug fixes

## 2021-07-23  1.19.0
- IStage extended with getStatusAsync method providing IStageStatus and method for getting match status
- Added Prologue value to StageType enum
- Added improvement for connection recovery when disconnection happens
- Fix: implemented safe release of all internal SemaphoreSlims
- Fix: added handling of variant market description when different market id between requesting and received id

## 2021-07-07  1.18.1
- Fix: problem within SemaphorePool not acquiring new semaphore - waiting indefinitely
- Fix: setting configuration via CustomConfigurationBuilder
- Fix: exception with modified competitor players list

## 2021-06-23  1.18.0
- Added OperationManager to provide option to set sdk values
- Added option to ignore sport event status from timeline endpoint for BetPal events

## 2021-06-15  1.17.0
- Added pitcher, batter, pitch_count, pitches_seen, total_hits, total_pitches to SportEventStatus.Properties
- PeriodScore - match status code 120 mapped to penalties
- Improved importing, exporting competitors
- Improved getting competitor players
- Fix: throwing exception when no market description received from API for variant markets

## 2021-05-28  1.16.0
- Extended IMatch with GetEventTimeline for single culture
- IOutcomeProbabilities extended with property AdditionalProbabilities
- IMarketWithProbabilities extended with MarketMetaData
- Extended ITournament with GetScheduleAsync
- Fix: Issue retrieving child stages from parent using  GetStagesAsync() method
- Fix: corrected which market description is returned for variant markets
- Improvement: optimized fetching of player/competitor profiles

## 2021-04-29  1.15.0
- Added ISportDataProvider.GetPeriodStatusesAsync to fetch period summary for stages
- Extended ICompetitionStatus with PeriodOfLadder
- Added support for SportEventStatus PeriodOfLeader - added to Properties
- Improved handling of outright market mappings (before for some markets there were no mappings returned)
- Improved merging of Competitor data
- Fix: IMarketWithOdds.GetValidMappings() returns incorrect results

## 2021-04-07  1.14.1
- Fix: parsing TeamStatistics for SportEventStatus

## 2021-03-31  1.14.0
- Added IEventChangeManager to IOddsFeed for periodical fixture and result change updates
- Changed type of property ITimelineEvent.Player from IPlayer to IEventPlayer (breaking change)
- Added IEventPlayer.Bench property in ITimelineEvent.Player property
- Added IGoalScorer.Method in ITimelineEvent.GoalScorer property
- Added property ICompetitor.ShortName
- Added property IProductInfo.IsInLiveMatchTracker
- Improved how internal cache sport event items handle competitor lists
- Changed ExportableCurrentSeasonInfoCI, ExportableGroupCI, ExportableTournamentInfoCI to return Competitors ids as list of string instead of ExportableCompetitorCI
- Improved caching of competitors data on tournaments, seasons
- Reverted populating Round Name, GroupName and PhaseOrGroupLongName to exactly what is received from API
- Updated FixtureChangeType - also if not specified in fixtureChange message returns FixtureChangeType.NA
- Added period_of_leader to the SportEventStatus.Properties
- Added StartTime, EndTime and AamsId to the IMarketMetaData
- Improved connection handling when disconnected
- Improved merging tournament groups (when no group name or id)
- Added some logs for errors when using unaccepted token
- Fix: WNS event ids can have negative value
- Fix: merging tournament groups
- Fix: TeamStatistics returned 0 when actually null
- Fix: EventResult Home and Away Score could be returned as 0, when actually null
- Fix: exporting/importing season data
- Fix: resolution of dependencies - removed some Guard check

## 2021-02-09  1.13.0
- Added ISportDataProvider.GetLotteriesAsync
- Improved translation of market names (upgraded referenced library Humanizer to 2.8.26 and Dawn.Guard to 1.12.0)
- Added support for eSoccer - returns SoccerEvent instead of Match
- Added support for simple_team urn
- Adding removal of obsolete tournament groups
- Improved internal sdk processing. API calls for markets done only per user request. Optimized feed message validation.
- Fix: for a case when sdk does not auto recover after disconnect

## 2020-12-15  1.12.0
- Extended ILottery with GetDraws to return list of IDraw (not just ids)
- Extended ISportDataProvider with GetSportEvent so also IDraw can be obtained
- Fix: getting fixture from API when result is tournamentInfo
- Fix: added removal of obsolete EventTimeline events
- Fix: not getting tournament data for stages

## 2020-12-04  1.11.1
- Fix: getting ScheduleForDay endpoint when no events throw exception
- Fix: missing totalStatistics in SoccerStatus.Statistics
- Fix: soccer events not instance of ISoccerEvent
- Fix: getting null fetching sport and parent stage info for stages

## 2020-11-13  1.11.0
- Added new stage types in StageType enum (Practice, Qualifying, QualifyingPart, Lap)
- Added CashoutStatus to IMarketWithProbabilitiesV1
- Fix: loading sportEventType and stageType from parent stage if available
- Fix: IMarketWithOdds.GetMappedMarketIDsAsync() returns multiple mappings
- Fix: exception casting TournamentInfoCI to StageCI

## 2020-10-13  1.10.0
- IRound - GroupName renamed to Group, added GroupName property (breaking change)
- IStage - added GetAdditionalParentStages, GetStageType (breaking change - result changed from SportEventType to StageType)
- IEventResult extended with Distance and CompetitorResults
- ICompetition extended with GetLiveOdds and GetSportEventType property
- Added Course to the IVenue
- Added Coverage to IMatch
- Improvements in recovery manager
- Added support for markets with outcome_type=competitors
- Make replay manager available before the feed is open
- Improved connection error handling and reporting
- Fix: exception thrown when there are no fixture changes
- Fix: soccer events not instance of ISoccerEvent
- Fix: entities null even though data is present on the API
- Fix: event status enumeration

## 2020-08-19  1.9.0
- Extended SeasonInfo with startDate, endDate, year and tournamentId
- Fix: special case when recovery status does not reflect actual state - results in wrong triggering ProducerUp-Down event
- Fix: URN.TryParse could throw unhandled exception
- Fix: several issues with CustomBet(Manager) fixed
- Fix: Export-Import breaks on missing data
- Fix: Lottery throws exception when no schedule is obtained from api
- Fix: missing nodeId in snapshot_complete routing key
- Fix: SportDataProvider.GetActiveTournaments returned null
- Fix: SportEventCache: improved locking mechanism on period fetching of schedule for a date
- Fix: reloading market description in case variant descriptions are not available
- Improved logging of initial message processing

## 2020-07-09  1.8.0
- Added GetSportAsync() and GetCategoryAsync() to ICompetitor interface
- Throttling recovery requests
- Fix: support Replay routing keys without node id
- Fix: calling Replay fixture endpoint with node id

## 2020-06-24  1.7.0
- Added support for configuring HTTP timeout
- Added overloaded methods for fixture and result changes with filters
- Updated supported languages
- Removed logging of feed message for disabled producers
- Exposed RawMessage on UnparsableMessageEventArgs
- Changed retention policy for variant market cache 
- Improved reporting of invalid message interest combinations 
- Fix: Synchronized Producer Up/Down event dispatching
- Fix: Disposing of Feed instance
- Fix: Permanent failure to open connection to feed

## 2020-05-11  1.6.0
- Added FullName, Nickname and CountryCode to IPlayerProfile
- Added support for result changes endpoint
- IMatchStatus provide nullable Home and Away score (extended with IMatchStatusV1)
- Fix: MaxRecoveryTime is properly used to check for timeouts

## 2020-04-16  1.5.0
- Added GetScheduleAsync to the BasicTournament
- Added bookmakerId to the ClientProperties
- Fix: fixture endpoint on Replay
- Fix: refreshing categories after complete sport data cache reload

## 2020-03-25  1.4.1
- Changed Replay API URL

## 2020-03-23  1.4.0
- Fix: invalid timestamp for cashout probabilities
- Fix: handle settlement markets without outcomes
- Fix: EventRecoveryCompleted is properly raised when snapshot completes

## 2020-03-16  1.3.0
- Fix: added IOutcomeSettlement.OutcomeResult instead of IOutcomeSettlement.Result (obsolete)
- Fix: competitor references for seasons
- Fix: failing API requests on some configurations

## 2020-02-18  1.2.0
- Added State to the Competitor
- Added State to the Venue
- Extended ISportInfoProvider.DeleteSportEventFromCache with option to delete sport event status
- Improved logging for connection errors
- Improved fetching fixtures for Replay environment
- Fix: calling variant endpoint only if user requests market data
- Fix: NullPointerException in ReplayManager

## 2020-01-15  1.1.1
- Fix: DI error for FeedRecoveryManager

## 2020-01-14  1.1.0
- Added new Replay API endpoints
- Updated DemoProject to use ILoggerFactory
- Added metrics to SpecificEntityWriter in DemoProject
- Fix: fetching outcome mappings for special markets that exists only on dynamic variant endpoint

## 2020-01-06  1.0.0
- Port of UF SDK to .NET Standard 2.1
- Replaced Metrics.NET with App.Metrics
- Replaced Code.Contracts with Dawn.Guard conditions
- Replaced Common.Logging with Microsoft.Extensions.Logging
- Upgraded RabbitMQ.Client to v5.1.2
- Upgraded Newtonsoft.Json to v12.0.3
- Upgraded Unity to 5.11.3
- Removed obsolete methods and properties
- Merged V1, V2, ... extended interfaces into base one

