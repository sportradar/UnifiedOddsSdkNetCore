/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Class MatchCI
    /// </summary>
    /// <seealso cref="CompetitionCI" />
    /// <seealso cref="IMatchCI" />
    internal class MatchCI : CompetitionCI, IMatchCI
    {
        /// <summary>
        /// The season
        /// </summary>
        private SeasonCI _season;
        /// <summary>
        /// The tournament round
        /// </summary>
        private RoundCI _tournamentRound;
        /// <summary>
        /// The tournament identifier
        /// </summary>
        private URN _tournamentId;
        /// <summary>
        /// The fixture
        /// </summary>
        private IFixture _fixture;
        /// <summary>
        /// The event timeline
        /// </summary>
        private EventTimelineCI _eventTimeline;
        /// <summary>
        /// The delayed info
        /// </summary>
        private DelayedInfoCI _delayedInfo;
        /// <summary>
        /// The coverage info
        /// </summary>
        private CoverageInfoCI _coverageInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchCI"/> class
        /// </summary>
        /// <param name="id">A <see cref="URN" /> specifying the id of the sport event associated with the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixture timestamps</param>
        public MatchCI(URN id,
                       IDataRouterManager dataRouterManager,
                       ISemaphorePool semaphorePool,
                       CultureInfo defaultCulture,
                       MemoryCache fixtureTimestampCache)
            : base(id, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchCI"/> class
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixture timestamps</param>
        public MatchCI(MatchDTO eventSummary,
                       IDataRouterManager dataRouterManager,
                       ISemaphorePool semaphorePool,
                       CultureInfo currentCulture,
                       CultureInfo defaultCulture,
                       MemoryCache fixtureTimestampCache)
            : base(eventSummary, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Merge(eventSummary, currentCulture, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchCI"/> class
        /// </summary>
        /// <param name="fixture">The fixture data</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixture timestamps</param>
        public MatchCI(FixtureDTO fixture,
                        IDataRouterManager dataRouterManager,
                        ISemaphorePool semaphorePool,
                        CultureInfo currentCulture,
                        CultureInfo defaultCulture,
                        MemoryCache fixtureTimestampCache)
            : base(fixture, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Merge(fixture, currentCulture, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchCI"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableMatchCI" /> specifying the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixture timestamps</param>
        public MatchCI(ExportableMatchCI exportable,
            IDataRouterManager dataRouterManager,
            ISemaphorePool semaphorePool,
            CultureInfo defaultCulture,
            MemoryCache fixtureTimestampCache)
            : base(exportable, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
            _season = exportable.Season != null ? new SeasonCI(exportable.Season) : null;
            _tournamentRound = exportable.TournamentRound != null ? new RoundCI(exportable.TournamentRound) : null;
            _tournamentId = exportable.TournamentId != null ? URN.Parse(exportable.TournamentId) : null;
            _fixture = exportable.Fixture != null ? new Fixture(exportable.Fixture) : null;
            _eventTimeline = exportable.EventTimeline != null ? new EventTimelineCI(exportable.EventTimeline) : null;
            _delayedInfo = exportable.DelayedInfo != null ? new DelayedInfoCI(exportable.DelayedInfo) : null;
            _coverageInfo = exportable.CoverageInfo != null ? new CoverageInfoCI(exportable.CoverageInfo) : null;
        }

        /// <summary>
        /// Get season as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        public async Task<SeasonCI> GetSeasonAsync(IEnumerable<CultureInfo> cultures)
        {
            var wantedCultures = cultures as CultureInfo[] ?? cultures.ToArray();
            if (_season != null && _season.HasTranslationsFor(wantedCultures))
            {
                return _season;
            }
            await FetchMissingSummary(wantedCultures, false).ConfigureAwait(false);
            return _season;
        }

        /// <summary>
        /// Get tournament round as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        public async Task<RoundCI> GetTournamentRoundAsync(IEnumerable<CultureInfo> cultures)
        {
            var cultureInfos = cultures as CultureInfo[] ?? cultures.ToArray();
            if (_tournamentRound != null && _tournamentRound.HasTranslationsFor(cultureInfos))
            {
                return _tournamentRound;
            }
            await FetchMissingSummary(cultureInfos, false).ConfigureAwait(false);
            return _tournamentRound;
        }

        /// <summary>
        /// Get tournament identifier as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{URN}" /> representing the asynchronous operation</returns>
        public async Task<URN> GetTournamentIdAsync(IEnumerable<CultureInfo> cultures)
        {
            if (_tournamentId != null)
            {
                return _tournamentId;
            }
            var cultureInfos = cultures.ToList();
            await FetchMissingSummary(cultureInfos, false).ConfigureAwait(false);
            if (_tournamentId == null)
            {
                await FetchMissingFixtures(cultureInfos).ConfigureAwait(false);
            }
            return _tournamentId;
        }

        /// <summary>
        /// Get fixture as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        /// <remarks>A Fixture is a sport event that has been arranged for a particular time and place</remarks>
        public async Task<IFixture> GetFixtureAsync(IEnumerable<CultureInfo> cultures)
        {
            await FetchMissingFixtures(cultures).ConfigureAwait(false);
            return _fixture;
        }

        /// <summary>
        /// Gets the associated event timeline
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <remarks>The timeline is cached only after the event status indicates that the event has finished</remarks>
        public async Task<EventTimelineCI> GetEventTimelineAsync(IEnumerable<CultureInfo> cultures)
        {
            var wantedCultures = cultures as IList<CultureInfo> ?? cultures.ToList();
            if (_eventTimeline == null || !_eventTimeline.IsFinalized)
            {
                // if we don't have timeline or is not yet finalized, all cultures should be fetched; otherwise only missing ones
            }
            else
            {
                wantedCultures = LanguageHelper.GetMissingCultures(wantedCultures, _eventTimeline.FetchedCultureInfos).ToList();
            }

            var tasks = wantedCultures.Select(s => DataRouterManager.GetInformationAboutOngoingEventAsync(Id, s, this));
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return _eventTimeline;
        }

        /// <summary>
        /// Asynchronously gets <see cref="DelayedInfoCI" /> instance providing delayed info
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<DelayedInfoCI> GetDelayedInfoAsync(IEnumerable<CultureInfo> cultures)
        {
            await FetchMissingFixtures(cultures).ConfigureAwait(false);
            return _delayedInfo;
        }

        /// <summary>
        /// Asynchronously gets <see cref="CoverageInfoDTO" /> instance providing coverage info
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<CoverageInfoCI> GetCoverageInfoAsync(IEnumerable<CultureInfo> cultures)
        {
            await FetchMissingFixtures(cultures).ConfigureAwait(false);
            return _coverageInfo;
        }

        /// <summary>
        /// Merges the specified event summary
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void Merge(MatchDTO eventSummary, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    ActualMerge(eventSummary, culture);
                }
            }
            else
            {
                ActualMerge(eventSummary, culture);
            }
        }

        /// <summary>
        /// Merges the specified event summary
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="culture">The culture</param>
        private void ActualMerge(MatchDTO eventSummary, CultureInfo culture)
        {
            base.Merge(eventSummary, culture, false);

            if (eventSummary.Season != null)
            {
                if (_season == null)
                {
                    _season = new SeasonCI(eventSummary.Season, culture);
                }
                else
                {
                    _season.Merge(eventSummary.Season, culture);
                }
            }
            if (eventSummary.Round != null)
            {
                if (_tournamentRound == null)
                {
                    _tournamentRound = new RoundCI(eventSummary.Round, culture);
                }
                else
                {
                    _tournamentRound.Merge(eventSummary.Round, culture);
                }
            }
            if (eventSummary.Tournament != null)
            {
                _tournamentId = eventSummary.Tournament.Id;
            }
            if (eventSummary.Coverage != null)
            {
                _coverageInfo = new CoverageInfoCI(eventSummary.Coverage);
            }
        }

        /// <summary>
        /// Merges the specified fixture
        /// </summary>
        /// <param name="fixtureDTO">The fixture</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public new void MergeFixture(FixtureDTO fixtureDTO, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    ActualMergeFixture(fixtureDTO, culture);
                }
            }
            else
            {
                ActualMergeFixture(fixtureDTO, culture);
            }
        }

        /// <summary>
        /// Merges the specified fixture
        /// </summary>
        /// <param name="fixture">The fixture</param>
        /// <param name="culture">The culture</param>
        private void ActualMergeFixture(FixtureDTO fixture, CultureInfo culture)
        {
            base.MergeFixture(fixture, culture, false);
            Merge(fixture, culture, false);

            _fixture = new Fixture(fixture);
            if (fixture.DelayedInfo != null)
            {
                if (_delayedInfo == null)
                {
                    _delayedInfo = new DelayedInfoCI(fixture.DelayedInfo, culture);
                }
                else
                {
                    _delayedInfo.Merge(fixture.DelayedInfo, culture);
                }
            }
            if (fixture.Coverage != null)
            {
                _coverageInfo = new CoverageInfoCI(fixture.Coverage);
            }
        }

        /// <summary>
        /// Merges the specified fixture
        /// </summary>
        /// <param name="timelineDTO">The match timeline</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void MergeTimeline(MatchTimelineDTO timelineDTO, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    ActualMergeTimeline(timelineDTO, culture);
                }
            }
            else
            {
                ActualMergeTimeline(timelineDTO, culture);
            }
        }

        /// <summary>
        /// Merges the specified fixture
        /// </summary>
        /// <param name="timelineDTO">The match timeline</param>
        /// <param name="culture">The culture</param>
        private void ActualMergeTimeline(MatchTimelineDTO timelineDTO, CultureInfo culture)
        {
            if (timelineDTO.SportEvent is MatchDTO matchDTO)
            {
                ActualMerge(matchDTO, culture);
            }
            if (_eventTimeline == null)
            {
                _eventTimeline = new EventTimelineCI(timelineDTO, culture);
            }
            else
            {
                _eventTimeline.Merge(timelineDTO, culture);
            }
            if (timelineDTO.CoverageInfo != null)
            {
                _coverageInfo = new CoverageInfoCI(timelineDTO.CoverageInfo);
            }
        }

        protected override async Task<T> CreateExportableCIAsync<T>()
        {
            var exportable = await base.CreateExportableCIAsync<T>();

            if (exportable is ExportableMatchCI match)
            {
                match.Season = _season != null
                    ? await _season.ExportAsync().ConfigureAwait(false)
                    : null;
                match.TournamentRound = _tournamentRound != null
                    ? await _tournamentRound.ExportAsync().ConfigureAwait(false)
                    : null;
                match.TournamentId = _tournamentId?.ToString();
                match.Fixture = _fixture != null
                    ? await ((Fixture)_fixture).ExportAsync().ConfigureAwait(false)
                    : null;
                match.EventTimeline = _eventTimeline != null
                    ? await _eventTimeline.ExportAsync().ConfigureAwait(false)
                    : null;
                match.DelayedInfo =
                    _delayedInfo != null ? await _delayedInfo.ExportAsync().ConfigureAwait(false) : null;
                match.CoverageInfo = _coverageInfo != null
                    ? new ExportableCoverageInfoCI
                    {
                        CoveredFrom = _coverageInfo.CoveredFrom,
                        Includes = _coverageInfo.Includes,
                        IsLive = _coverageInfo.IsLive,
                        Level = _coverageInfo.Level
                    }
                    : null;
            }

            return exportable;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public override async Task<ExportableCI> ExportAsync() => await CreateExportableCIAsync<ExportableMatchCI>().ConfigureAwait(false);
    }
}
