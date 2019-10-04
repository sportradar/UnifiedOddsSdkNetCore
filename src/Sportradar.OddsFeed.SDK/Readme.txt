A UnifiedOdds Feed SDK library

For more information please contact support@sportradar.com or visit https://iodocs.betradar.com/unifiedsdk/index.html

Important: Version 1.1.0.0 includes breaking changes, below are the steps needed to update 3rd party code.
1. replace/rename ISportEvent to ICompetition (make sure to search whole words with 'Match case' enabled)
2. replace/rename ISportEntity to ISportEvent (make sure to search whole words with 'Match case' enabled)
3. resolve any remaining issues


CHANGE LOG:
2019-09-05  1.24.0.0
Exposed option to delete old matches from cache - introduced ISportDataProviderV4
Loading home and away penalty score from penalty PeriodScore if present
Fix: return types in ISportDataProviderV3 (breaking change)
Fix: updated CustomConfigurationBuilder not to override pre-configured values
Fix: OutcomeDefinition null for variant markets
Fix: ProfileCache - CommunicationException is not wrapped in CacheItemNotFoundException
Fix: schedule date between normal and virtual feed synchronized
Fix: SportDataProvider methods invokes API requests for correct language

2019-07-19  1.23.1.0
Fix: ReplayFeed init exception

2019-07-18  1.23.0.0
Added Gender property to the IPlayerProfileV1
Added DecidedByFed property to the IMatchStatusV2
Added RaceDriverProfile property to the ICompetitorV2
Added GetExhibitionGamesAsync() to the IBasicTournamentV1 and ITournamentV1
Added Id property to the IGroupV1
Added TeamId and Name properties to the ITeamStatisticsV1
Added support for List sport events - ISportDataProvider extended with ISportDataProviderV2
Added support for TLS 1.2
Added GetAvailableTournamentsAsync(sportId) and GetActiveTournamentsAsync() to the ISportDataProviderV3
Fix: when sdk connects and API is down, UF SDK waits for next alive to make recovery
Fix: not loading variant market data in multi-language scenario
Fix: removed making whoami request in Feed ctor
Fix: on Feed.Open exception, the Open state is reset
Fix: NPE for validating market mappings when there are none

2019-06-21  1.22.0.0
Added GetStartTimeTbdAsync and GetReplacedByAsync to the ISportEventV1
Added properties StripesColor, SplitColor, ShirtType and SleeveDetail to the IJerseyV1
Improved on updating when new outcomes are available (outrights)
Exposed option for user to receive raw feed and api data
PeriodScore.MatchStatusCode no more obsolete
Fix: unnecessary api calls for competitor profiles

2019-06-07  1.21.0.0
Added property Gender to the ICompetitorV1
Added property Division to the ITeamCompetitorV1
Added property StreamUrl to the ITvChannelV1
Added property Phase to the IRoundV2
ICompetitionStatus.Status no more obsolete (fixed underlining issue)
Improved caching of variant market descriptions
Fix: caching the category without tournament failed
Fix: event status and score update issue
Fix: IMarketDescription interface exposed to user
Fix: error fetching data for sport event throws exception when enabled exception handling strategy
Fix: ReplayManager - the parameter start_time fixed

2019-05-23  1.20.1.0
Fix: unable to initialize feed

2019-05-22  1.20.0.0
Added support for custom bets
Added CustomBetManager, MarketDescriptionManager and EventRecoveryCompleted event to the IOddsFeedV2 (extends IOddsFeed)
Added GetCompetition method without sportId parameter to the ISportDataProviderV1 (extends ISportDataProvider)
Added GetFixtureChanges to the ISportsInfoManagerV1 interface (extends ISportsInfoManager)
Exposed OutcomeDefinition on IOutcomeV1 (extends IOutcome)
Exposed option to reload market descriptions
Fix: creating session with MessageInterest.SpecificEventOnly
Fix: exception when getting data for tournament with no schedule
Fix: calling TournamentInfoCI.GetScheduleAsync() from multiple threads
Fix: IMarketMappingData, IOutcomeMappingData moved from internal to public namespace

2019-04-18  1.19.0.0
Added property GroupId to the Round interface - IRound extended with IRoundV1
Improved handling of SportEventStatus updates
Improved name fetching for competitors
Fix: fixed legacy market mappings
Fix: incorrect message validation

2019-04-08  1.18.0.0
Added GetDisplayIdAsync to the IDrawV1
Added support for non-cached fixture endpoint
Improved fetching logic for the summary endpoint
Made IAlive interface internal
Fix: handling pre:outcometext and simpleteam ids in cache
Fix: IMarket.GetNameAsync - removed concurrency issue
Fix: added missing ConfigureAwait(false) to async functions

2019-03-12  1.17.0.0
Added property Grid to the EventResult interface - IEventResult extended with IEventResultV1
Added property AamsId to the Reference - IReference extended with IReferenceV1
ICompetitionStatus.Status deprecated
Instead added GetEventStatusAsync to ICompetition - extended with ICompetitionV1
IMatch extended with IMatchV1, IStage extended with IStageV1
Added Pitchers to the ISportEventConditionsV1 interface
Added enum option EventStatus.MatchAboutToStart
Added support for simpleteam competitors and related API calls
Take recovery max window length for each producer from api (all producers)
Added runParallel argument on StartReplay method on ReplayManager
Improved speed of data distribution among caches (data received from api)
Fix: IMarket names dictionary changed to ConcurrentDictionary
Fix: how PeriodScore data is saved and exposed
Fix: if the venue data is obtained from date schedule is cached so no summary request is needed

2019-02-14  1.16.0.0
Exposed Timestamps on IMessage
Added RecoveryInfo (info about last recovery) to the Producer - extended with IProducerV1
Added support for replay feed to the Feed instance
Cache distribution goes only to caches that process specific dto type
Added SDK examples based on replay server
Fix: Sport.Categories now returns all categories, delayed fetching until needed
Fix: fixed legacy market mappings (mapping to Lo or Lcco market mapping)
Fix: calling GetSportAsync from multiple threads
Fix: Competitor returning null values for AssociatedPlayers

2019-01-07  1.15.0.0
ICoveredInfo extended with ICoveredInfoV1 - exposed CoveredFrom property
Added GetOutcomeType method to IMarketDefinition (to replace GetIncludesOutcomesOfType)
Competitor qualifier is loaded with summary (before fixture)
Fix: ICompetitor.References - fixture is fetched only if competitor references are explicitly requested by user
Fix: avoiding fetching fixture for BookingStatus when received via schedule
Fix: improved locking mechanism in SportEventStatusCache to avoid possible deadlock
Fix: added check before fetching summary for competitors when already prefetched
Other minor fixes and improvements

2018-12-18  1.14.0.0
IOddsChange extended with IOddsChangeV1 - added OddsGenerationProperties
Replay session using any token returns production replay summary endpoint
Added support for custom api hosts (recovery for producers uses custom urls)
Improved locking mechanism when merging data in cache items
Added support for custom api hosts (recovery for producers uses custom urls)
Added Season start time and end time to exposed dates
Renamed Staging to Integration environment
Updated examples
Fix: filling referenceIds
Fix: SportEventCache fix for requester merge (TournamentInfoCI cast problem)
Fix: updated BetSettlementCertainty enum values to better reflect values received in feed message

2018-11-16  1.13.0.0
Extended IOddFeed with IOddsFeedV1 - new property BookmakerDetails available on OddsFeed
Improved handling of competitor reference id(s)
Removed purging of sportEventStatus from cache on betSettlement message
Fix: minimized rest api calls - removed calls for eng, when not needed (requires language to be set in configuration)
Fix: when sdk returned field null value although it is present; also avoids repeated api request

2018-10-17  1.12.0.0
Extended ITimelineEvent with ITimelineEventV1 - added MatchClock and MatchStatusCode properties
Extended IMatchStatus with IMatchStatusV1 - added HomePenaltyScore and AwayPenaltyScore (used in Ice Hockey)
Added more logs in ProducerRecoveryManager when status changes
Added more explanation in log message when wrong timestamp is set in AddTimestampBeforeDisconnect method
Added warn message for negative nodeId (negative id is reserved for internal use only)
Removed logging of response headers in LogProxy class on debug level
Fix: calling the summary with nodeId if specified (on replay server) - sport event status changes during replay of the match
Fix: Improvement of fetching and caching of SportEventStatus when called directly
Fix: schedule on CurrentSeasonInfo was not filled with data from API

2018-09-18  1.11.0.0
Added new event status: EventStatus.Interrupted
Market / outcome name construction from competitor abbreviations (if missing)
Improved logging in DataRouterManager
Improved recovery procedure (to avoid The recovery operation is already running exception)
Fix: exception during fetching event fixture or summary is exposed to user (for ExceptionHandlingStrategy.THROW setting)
Fix: error when associated players method returning null
Fix: MarketMapping for variant markets returns correctly
Fix: remove sportEvent from cache on betStop and betSettlement message (to purge its sportEventStatus)

2018-08-24  1.10.0.0
Added support for outcome odds in different formats
Introduced IOutcomeOddsV1 extending IOutcomeOdds with method GetOdds(OddsDisplayType)
ReplayServer example updated with PlayScenario
Increased RabbitMQ.Client library to 3.6.9
Fix: repaired Schedule and ScheduleEnd times for simple_tournament events
Fix: ISeason.TournamentInfo.Names contains all fetched values (before only first)

2018-07-23  1.9.0.0
Exposed property TeamFlag and HomeOrAwayTeam on IPlayerOutcomeOdds
Added support for Virtual Sports In-play message type
Added Pitcher info on SportEventConditions
Update: removing draw events from cache on DrawStatus change
Update: variant markets expiration time set to 1h
Update: added logging for sync fetcher
Fix: internally updating competitors when competitors changes on sportEvent
Fix: generation of sportEvent name with $event name template
Fix: DataRouterManager - updated logs when fetching tournament seasons
Fix: generating mapped outcomes with $score sov which dont have attribute is_flex_score
Fix: thread safety issue in ProductRecoveryManager class
Fix: statistics properties loaded from config section when needed; added CacheManager statistics
Fix: NameProvider.GetOutcomeNameFromProfileAsync returns result for ILongTermEvent without competitors

2018-06-11	1.8.0.0
Added property RotationNumber to ReferenceIds
Mapped outcomes supports translatable name
Added support for multiple market and outcome mappings
Added mapped marketId to the IOutcomeMapping
Outcome mappings for flex_score_markets take into account the score specifiers	
Removed checks for requestId on feed messages
Exposed ScheduledStartTimeChanges on IFixture
Ensured fixture change messages are dispatched only once
Improved generation of outcome names for sr:players
Improved handling of caching for simpleteam competitor and betradarId for simpleteams
In SportEventStatus in the Properties only int values are saved for Status and MatchStatus properties
Updating cached sport event booking status when user books event
Fix: Child stage links to parent stage or parent tournament
Fix: loading of EventResult
Fix: SportEventCache cannot fetch multiple times for the same date
Fix: ICompetition.GetConditionsAsync did not load for all cultures
Fix: saving-caching special tournament data
Fix: Venue on competition

2018-04-26  1.7.0.0
Added CountryCode property to IVenue
Added AdjustAfterAge property to Section and ConfigurationBuilders
Added local time check against bookmaker details response header
Added PhaseOrGroupLongName property to IRound
WNS endpoints are called only if WNS available
Improved handling of sport event status obtained from feed or API
All recovery methods takes into account the specified nodeId
Fix: missing market description or outcome no longer throws exception
Fix: loading of tournament data for season events
Fix: print of SportEventConditions
Fix: FixtureDTO mapping for ReplacedBy property
Internal: added exception handling strategy to DataRouterManager
Other minor fixes and improvements

2018-04-03	1.6.0.0
New configuration builders
Added support for replaying of events messages generated by a specific producer
Removed restrictions on speed and maxDelay in ReplayManager
Exposed RawMessage on a IEventMessage<T> (returns byte array received from feed)
Removed asynchronous call within Feed ctor
Added 'Postponed' value to enum EventStatus
IEventResult properties Points, Climber and Sprint marked obsolete
Introduced new decimal properties to IEventResult: PointsDecimal, WcPoints, ClimberDecimal and SprintDecimal
Added properties ReplacedBy, NextLiveTime and StartTimeTBD to IFixture
Fix: exception when season has no groups
Fix: getting TournamentInfo on Season instance
Fix: displaying correct CurrentSeasonInfo on ISeason
Fix: loading competitors for stage events, loading category for stage events
Fix: loading draw events for lottery;
Fix: loading draw fixture
Internal: improved how recovery is made
Internal: improvements in caches to avoid exceptions, locks, ...
Other minor fixes and improvements

2018-03-01  1.5.0.0
Exposed VoidReason on BetCancel, BetSettlement, ... (introduced IMarketCancel interface)
Added property Timestamp to all feed messages (time when message was generated)
Updated IPeriodScore to reflect new rest and feed periodScoreType
Added GetMatchStatusAsync on IPeriodScore and IEventResult (it returns the same value as on IMatchStatus)
Updated some types of properties on IEventResult to correctly reflect those on Sport API
Improved handling of recovery and producer status based on feed messages
Internal: CacheManager made non-static - now supports multiple feed instances
Internal: hardening of cache handling for automatically fetching data
DemoProject updated
Fix: updated how IReplayManager.AddSportEventToReplay behaves when startTime is not specified
Fix: fixed how variant markets are cached (includes player props markets)
Fix: ITournament.GetSeasonsAsync always returned null
Fix: running multiple feed instances at the same time or scenario open-close-open of the same instance
Fix: correctly handling of decimal value in outcome name with +/- name template
Other minor fixes and improvements, improved internal exception handling

2018-02-12  1.4.0.0
Added support for WNS/lottery (new IDraw and ILottery sport event)
Added property DelayedInfo to IMatch
Added property ProductsId to IMarketMapping
Internal: optimization of data distribution amoung sdk caches
Fix: MarketCacheProvider throwing exception when no market description found
Fix: handling of logging exception when error happens receiving data from Sports API
Other minor improvements and fixes

2018-01-29  1.3.0.0
Added support for $event name template
Added support for match booking
Updated support for variant markets
Removed log4net logging library and introduced Common.Logging library. For an example of configuration using log4net & Common.Logging please check the latest SDK example.
OddsFeedConfigurationBuilder enables the user to specify the connection should be made to staging environment
Fix: Category property on ISport and ITournament always returned null
Other minor improvements and bug fixes

2018-01-15  1.2.0.0
SportEvent hierarchy overhaul - 'stage' support
Improved recovery process - on slow message processing, producer is marked as down
Added Certainty to BetSettlement message
Added support for Market MetaData on oddsChange message
Added support for multiple Groups on BetStop message
Exposed additional market info through market definitions
Added support for SoccerEvent, SoccerStatus
Expanded PlayerProfile and Competitor to support Manager, Jersey info
Exposed event timeline on IMatch entity
Exposed tournament seasons on ITournament entity
Exposed methods for clearing cache data on ISportDataProvider
NodeId supports negative numbers
Added support for nodeId on Replay server on all methods
Internally: changed how caches works and added CacheManager

2017-11-06	1.1.8.0
Improved recovery process
Support for player markets, support for empty market messages
IOddsFeed - added Closed event, invoked when feed is forcebly closed
Added property Name to ICompetion
Exposed raw xml message on all feed message received events
Fix: %server specifier template loaded from competitor profile
Fix: stateful messages not dispatched when connecting to replay server
Fix: updating cache for SportEventStatus on feed messages

2017-08-04    1.1.7.0
Fixed bug when feed recovery gets interrupted when lasting longer then expected
Empty tournament names now allowed
Added 'maxRecoveryTime' to the config section

2017-07-31   1.1.6.0
CashoutProvider enabled on feed instance
Season - fixed multilanguage name support
Messages for disabled producers are no longer dispatched
Fixed clearing SportEventStatusCache on bet_settlement message
Fixed that on disconnection / on productdown event is raised correctly when no message arrives within specified timeout

Version 1.1.6.0 contains breaking changes:
Product enumeration has been replaced with IProducer interface in order to ensure automatic support of new producers
Some functionality previously exposed on the IOddsConfiguration interface has been moved to new IProducerManager interface. 

2017-06-15	1.1.5.0
All SportEventStatus properties available in Properties list
Ensured thread safety in SportDataCache when new Tournament data is added/updated
Updated how SportEvent or Tournament Season data is obtained and available for users
Product code for LCoO changed to 'pre'
Updated market mapping validator to be culture invariant

2017-06-01    1.1.4.0
Fixed deserializing of API messages for race details
Changed how sport event status is obtained internally, made available via ISportDataProvider
Added auto-close to feed when recovery request can not be made

2017-05-29	1.1.3.0
Changed support for wild card validity market mapping validity attributes - now the format "specifer_name~*.xx" is supported
Fixed a bug in session message handling which caused some of the messages were not dispatched to the user
Modified handling of error response codes returned by the recovery endpoints 

2017-05-23 	1.1.2.0
Added support for x.0, x.25, x.5, x.75 market mapping validity attribute
Added support for (%player) name template in market name descriptions

2017-04-21 	1.1.0.0
Breaking Changes:
IMarketWithOdds.IsFavourite renamed to IMarketWithOdds.IsFavorite
IMarketWithOdds.Outcomes renamed to IMarketWithOdds.OutcomeOdds
IMarketWithSettlement.Outcomes renamed to IMarketWithSettlement.OutcomeSettlements
ISportEvent interface renamed to ICompetition
ISportEntity interface renamed to ISportEvent
IOddsChange.BetStopReason: Type changed from enum to INamedValue
IOddsChange.BettingStatus: Type changed from enum to INamedValue
Introduced IOddsFeedConfigurationBuilder which must now be used in order to create IOddsFeedConfiguration
RecoveryRequestIssuer property on the IOddsFeed renamed and it’s type changed

New Features:
Improvements & bug fixes to recovery process
Initial support for upcoming products BetPal & PremiumCricket added
Enforcing that timestamp for recovery is not more than 72 hours in the past or in the future
Added support for connection to custom message broker / RESTful api
Optimization of resources used by the connection to the message broker

2017-04-19 	1.0.3.0
Support for flexible score markets
Added support for outcomes with composite ids
Void reason added to betsettlement messages
Market status property added to betstop messages
Minor fixed how recovery are called internaly

2017-03-29 	1.0.2.0
Fixed calling groups on Tournament when no groups are present
Fixed internal mapping for Odds messages without markets

2017-03-31 1.0.1.0
Removed UnalteredEvents event and message
Added encoding UTF-8 for logging in log4net.sdk.config (check DemoProject)
Fixed internal message processing pipeline for multi-session scenario
Implemented support for simple math expressions in market names
Removed Group from SportEvent
SportDataProvider - fixed retrieving TournamentSchedule
Made some properties of the REST entities translatable due to changes in the feed 
  
2017-02-06 Official version (1.0.0)
Updated fixture changes (added References to SportEvent and Competitor object)
Added event statuses
Added MatchStatus in SportEventStatus object
Added support for Replay Server
Added support for Market and Outcomes mappings
Improved how recoveries are made (user can specify when last message was processed)
New DemoProject examples added
Performance improvements

2016-11-21 Release candidate (0.9.1)
SDK available via nuget package manager
SDK merged into a single assembly which only exposes types required by the user
Additional market statuses added

2016-11-04 Release candidate (0.9.0)
Outright support
Added support for generating market names
Ensured no exceptions are thrown to the external callers
Improved recovery
Multiple log files for easier debugging
Support for specific event handlers
Added diagnostic tools to determine the cause of potential connection issues
Support for multiple languages
Improved caching of sport related data

2016-07-04 Beta Version
Added support for retrieving information about sport events associated with each odds message
Added support for initial synchronization with the feed
Added support for detection of the out-of-sync situation and automatic re-sync
BetStop messages now also contain identifiers of the markets which needs to be stopped (not just a market group)

2016-06-17 Initial Version (alpha)