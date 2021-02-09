UnifiedOdds Feed SDK .NET library (.NET Standard 2.1)

For more information please contact support@sportradar.com or visit https://iodocs.betradar.com/unifiedsdk/index.html

CHANGE LOG:
2021-02-09  1.13.0.0
Added ISportDataProvider.GetLotteriesAsync
Improved translation of market names (upgraded referenced library Humanizer to 2.8.26 and Dawn.Guard to 1.12.0)
Added support for eSoccer - returns SoccerEvent instead of Match
Added support for simple_team urn
Adding removal of obsolete tournament groups
Improved internal sdk processing. API calls for markets done only per user request. Optimized feed message validation.
Fix: for a case when sdk does not auto recover after disconnect

2020-12-15  1.12.0.0
Extended ILottery with GetDraws to return list of IDraw (not just ids)
Extended ISportDataProvider with GetSportEvent so also IDraw can be obtained
Fix: getting fixture from API when result is tournamentInfo
Fix: added removal of obsolete EventTimeline events
Fix: not getting tournament data for stages

2020-12-04  1.11.1.0
Fix: getting ScheduleForDay endpoint when no events throw exception
Fix: missing totalStatistics in SoccerStatus.Statistics
Fix: soccer events not instance of ISoccerEvent
Fix: getting null fetching sport and parent stage info for stages

2020-11-13  1.11.0.0
Added new stage types in StageType enum (Practice, Qualifying, QualifyingPart, Lap)
Added CashoutStatus to IMarketWithProbabilitiesV1
Fix: loading sportEventType and stageType from parent stage if available
Fix: IMarketWithOdds.GetMappedMarketIDsAsync() returns multiple mappings
Fix: exception casting TournamentInfoCI to StageCI

2020-10-13  1.10.0.0
IRound - GroupName renamed to Group, added GroupName property (breaking change)
IStage - added GetAdditionalParentStages, GetStageType (breaking change - result changed from SportEventType to StageType)
IEventResult extended with Distance and CompetitorResults
ICompetition extended with GetLiveOdds and GetSportEventType property
Added Course to the IVenue
Added Coverage to IMatch
Improvements in recovery manager
Added support for markets with outcome_type=competitors
Make replay manager available before the feed is open
Improved connection error handling and reporting
Fix: exception thrown when there are no fixture changes
Fix: soccer events not instance of ISoccerEvent
Fix: entities null even though data is present on the API
Fix: event status enumeration

2020-08-19  1.9.0.0
Extended SeasonInfo with startDate, endDate, year and tournamentId
FIx: special case when recovery status does not reflect actual state - results in wrong triggering ProducerUp-Down event
Fix: URN.TryParse could throw unhandled exception
Fix: several issues with CustomBet(Manager) fixed
Fix: Export-Import breaks on missing data
Fix: Lottery throws exception when no schedule is obtained from api
Fix: missing nodeId in snapshot_complete routing key
Fix: SportDataProvider.GetActiveTournaments returned null
Fix: SportEventCache: improved locking mechanism on period fetching of schedule for a date
Fix: reloading market description in case variant descriptions are not available
Improved logging of initial message processing

2020-07-09  1.8.0.0
Added GetSportAsync() and GetCategoryAsync() to ICompetitor interface
Throttling recovery requests
Fix: support Replay routing keys without node id
Fix: calling Replay fixture endpoint with node id

2020-06-24  1.7.0.0
Added support for configuring HTTP timeout
Added overloaded methods for fixture and result changes with filters
Updated supported languages
Removed logging of feed message for disabled producers
Exposed RawMessage on UnparsableMessageEventArgs
Changed retention policy for variant market cache 
Improved reporting of invalid message interest combinations 
Fix: Synchronized Producer Up/Down event dispatching
Fix: Disposing of Feed instance
Fix: Permanent failure to open connection to feed

2020-05-11  1.6.0.0
Added FullName, Nickname and CountryCode to IPlayerProfile
Added support for result changes endpoint
IMatchStatus provide nullable Home and Away score (extended with IMatchStatusV1)
Fix: MaxRecoveryTime is properly used to check for timeouts

2020-04-16  1.5.0.0
Added GetScheduleAsync to the BasicTournament
Added bookmakerId to the ClientProperties
Fix: fixture endpoint on Replay
Fix: refreshing categories after complete sport data cache reload

2020-03-25  1.4.1.0
Changed Replay API URL

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
