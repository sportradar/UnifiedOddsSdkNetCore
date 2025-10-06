// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events
{
    /// <summary>
    /// Class MatchCacheItem
    /// </summary>
    /// <seealso cref="CompetitionCacheItem" />
    /// <seealso cref="IMatchCacheItem" />
    internal class MatchCacheItem : CompetitionCacheItem, IMatchCacheItem
    {
        /// <summary>
        /// The season
        /// </summary>
        private SeasonCacheItem _season;
        /// <summary>
        /// The tournament round
        /// </summary>
        private RoundCacheItem _tournamentRound;
        /// <summary>
        /// The tournament identifier
        /// </summary>
        private Urn _tournamentId;
        /// <summary>
        /// The fixture
        /// </summary>
        private IFixture _fixture;
        /// <summary>
        /// The event timeline
        /// </summary>
        private EventTimelineCacheItem _eventTimeline;
        /// <summary>
        /// The delayed info
        /// </summary>
        private DelayedInfoCacheItem _delayedInfo;
        /// <summary>
        /// The coverage info
        /// </summary>
        private CoverageInfoCacheItem _coverageInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchCacheItem"/> class
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> specifying the id of the sport event associated with the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCacheStore">A <see cref="ICacheStore{T}"/> used to cache the sport events fixture timestamps</param>
        public MatchCacheItem(Urn id,
                              IDataRouterManager dataRouterManager,
                              ISemaphorePool semaphorePool,
                              CultureInfo defaultCulture,
                              ICacheStore<string> fixtureTimestampCacheStore)
            : base(id, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCacheStore)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchCacheItem"/> class
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="ICacheStore{T}"/> used to cache the sport events fixture timestamps</param>
        public MatchCacheItem(MatchDto eventSummary,
                              IDataRouterManager dataRouterManager,
                              ISemaphorePool semaphorePool,
                              CultureInfo currentCulture,
                              CultureInfo defaultCulture,
                              ICacheStore<string> fixtureTimestampCache)
            : base(eventSummary, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Merge(eventSummary, currentCulture, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchCacheItem"/> class
        /// </summary>
        /// <param name="fixture">The fixture data</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="ICacheStore{T}"/> used to cache the sport events fixture timestamps</param>
        public MatchCacheItem(FixtureDto fixture,
                              IDataRouterManager dataRouterManager,
                              ISemaphorePool semaphorePool,
                              CultureInfo currentCulture,
                              CultureInfo defaultCulture,
                              ICacheStore<string> fixtureTimestampCache)
            : base(fixture, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Merge(fixture, currentCulture, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatchCacheItem"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableMatch" /> specifying the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCacheStore">A <see cref="ICacheStore{T}"/> used to cache the sport events fixture timestamps</param>
        public MatchCacheItem(ExportableMatch exportable,
                              IDataRouterManager dataRouterManager,
                              ISemaphorePool semaphorePool,
                              CultureInfo defaultCulture,
                              ICacheStore<string> fixtureTimestampCacheStore)
            : base(exportable, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCacheStore)
        {
            _season = exportable.Season != null ? new SeasonCacheItem(exportable.Season) : null;
            _tournamentRound = exportable.TournamentRound != null ? new RoundCacheItem(exportable.TournamentRound) : null;
            _tournamentId = exportable.TournamentId != null ? Urn.Parse(exportable.TournamentId) : null;
            _fixture = exportable.Fixture != null ? new Fixture(exportable.Fixture) : null;
            _eventTimeline = exportable.EventTimeline != null ? new EventTimelineCacheItem(exportable.EventTimeline) : null;
            _delayedInfo = exportable.DelayedInfo != null ? new DelayedInfoCacheItem(exportable.DelayedInfo) : null;
            _coverageInfo = exportable.CoverageInfo != null ? new CoverageInfoCacheItem(exportable.CoverageInfo) : null;
        }

        /// <summary>
        /// Get season as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        public async Task<SeasonCacheItem> GetSeasonAsync(IEnumerable<CultureInfo> cultures)
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
        public async Task<RoundCacheItem> GetTournamentRoundAsync(IEnumerable<CultureInfo> cultures)
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
        /// <returns>A <see cref="Task{Urn}" /> representing the asynchronous operation</returns>
        public async Task<Urn> GetTournamentIdAsync(IEnumerable<CultureInfo> cultures)
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
        public async Task<EventTimelineCacheItem> GetEventTimelineAsync(IEnumerable<CultureInfo> cultures)
        {
            var wantedCultures = cultures as ICollection<CultureInfo> ?? cultures.ToList();
            if (_eventTimeline == null || !_eventTimeline.IsFinalized)
            {
                // if we don't have timeline or is not yet finalized, all cultures should be fetched; otherwise only missing ones
            }
            else
            {
                wantedCultures = LanguageHelper.GetMissingCultures(wantedCultures, _eventTimeline.FetchedCultureInfos);
            }

            var tasks = wantedCultures.Select(s => DataRouterManager.GetInformationAboutOngoingEventAsync(Id, s, this));
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return _eventTimeline;
        }

        /// <summary>
        /// Asynchronously gets <see cref="DelayedInfoCacheItem" /> instance providing delayed info
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<DelayedInfoCacheItem> GetDelayedInfoAsync(IEnumerable<CultureInfo> cultures)
        {
            await FetchMissingFixtures(cultures).ConfigureAwait(false);
            return _delayedInfo;
        }

        /// <summary>
        /// Asynchronously gets <see cref="CoverageInfoDto" /> instance providing coverage info
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<CoverageInfoCacheItem> GetCoverageInfoAsync(IEnumerable<CultureInfo> cultures)
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
        public void Merge(MatchDto eventSummary, CultureInfo culture, bool useLock)
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
        private void ActualMerge(MatchDto eventSummary, CultureInfo culture)
        {
            base.Merge(eventSummary, culture, false);

            if (eventSummary.Season != null)
            {
                if (_season == null)
                {
                    _season = new SeasonCacheItem(eventSummary.Season, culture);
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
                    _tournamentRound = new RoundCacheItem(eventSummary.Round, culture);
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
                _coverageInfo = new CoverageInfoCacheItem(eventSummary.Coverage);
            }
        }

        /// <summary>
        /// Merges the specified fixture
        /// </summary>
        /// <param name="fixtureDto">The fixture</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public new void MergeFixture(FixtureDto fixtureDto, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    ActualMergeFixture(fixtureDto, culture);
                }
            }
            else
            {
                ActualMergeFixture(fixtureDto, culture);
            }
        }

        /// <summary>
        /// Merges the specified fixture
        /// </summary>
        /// <param name="fixture">The fixture</param>
        /// <param name="culture">The culture</param>
        private void ActualMergeFixture(FixtureDto fixture, CultureInfo culture)
        {
            base.MergeFixture(fixture, culture, false);
            Merge(fixture, culture, false);

            _fixture = new Fixture(fixture);
            if (fixture.DelayedInfo != null)
            {
                if (_delayedInfo == null)
                {
                    _delayedInfo = new DelayedInfoCacheItem(fixture.DelayedInfo, culture);
                }
                else
                {
                    _delayedInfo.Merge(fixture.DelayedInfo, culture);
                }
            }
            if (fixture.Coverage != null)
            {
                _coverageInfo = new CoverageInfoCacheItem(fixture.Coverage);
            }
        }

        /// <summary>
        /// Merges the specified fixture
        /// </summary>
        /// <param name="timelineDto">The match timeline</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void MergeTimeline(MatchTimelineDto timelineDto, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    ActualMergeTimeline(timelineDto, culture);
                }
            }
            else
            {
                ActualMergeTimeline(timelineDto, culture);
            }
        }

        /// <summary>
        /// Merges the specified fixture
        /// </summary>
        /// <param name="timelineDto">The match timeline</param>
        /// <param name="culture">The culture</param>
        private void ActualMergeTimeline(MatchTimelineDto timelineDto, CultureInfo culture)
        {
            if (timelineDto.SportEvent is MatchDto matchDto)
            {
                ActualMerge(matchDto, culture);
            }
            if (_eventTimeline == null)
            {
                _eventTimeline = new EventTimelineCacheItem(timelineDto, culture);
            }
            else
            {
                _eventTimeline.Merge(timelineDto, culture);
            }
            if (timelineDto.CoverageInfo != null)
            {
                _coverageInfo = new CoverageInfoCacheItem(timelineDto.CoverageInfo);
            }
        }

        protected override async Task<T> CreateExportableBaseAsync<T>()
        {
            var exportable = await base.CreateExportableBaseAsync<T>();

            if (exportable is ExportableMatch match)
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
                                         ? new ExportableCoverageInfo
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
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public override async Task<ExportableBase> ExportAsync()
        {
            return await CreateExportableBaseAsync<ExportableMatch>().ConfigureAwait(false);
        }
    }
}
