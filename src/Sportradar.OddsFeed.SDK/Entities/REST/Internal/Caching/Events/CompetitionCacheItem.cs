// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events
{
    /// <summary>
    /// Class CompetitionCacheItem
    /// </summary>
    /// <seealso cref="ISportEventCacheItem" />
    /// <seealso cref="ICompetitionCacheItem" />
    internal class CompetitionCacheItem : SportEventCacheItem, ICompetitionCacheItem
    {
        /// <summary>
        /// The booking status
        /// </summary>
        private BookingStatus? _bookingStatus;
        /// <summary>
        /// The venue
        /// </summary>
        private VenueCacheItem _venue;
        /// <summary>
        /// The conditions
        /// </summary>
        private SportEventConditionsCacheItem _conditions;
        /// <summary>
        /// The competitors
        /// </summary>
        public IEnumerable<Urn> Competitors;
        /// <summary>
        /// The reference identifier
        /// </summary>
        private ReferenceIdCacheItem _referenceId;
        /// <summary>
        /// The competitors qualifiers
        /// </summary>
        private IDictionary<Urn, string> _competitorsQualifiers;
        /// <summary>
        /// The competitors references
        /// </summary>
        private IDictionary<Urn, ReferenceIdCacheItem> _competitorsReferences;

        private string _liveOdds;

        private SportEventType? _sportEventType;

        private StageType? _stageType;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionCacheItem"/> class
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> specifying the id of the sport event associated with the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixtureDto</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ...)</param>
        /// <param name="fixtureTimestampCacheStore">A <see cref="ICacheStore{T}"/> used to cache the sport events fixtureDto timestamps</param>
        public CompetitionCacheItem(Urn id,
                             IDataRouterManager dataRouterManager,
                             ISemaphorePool semaphorePool,
                             CultureInfo defaultCulture,
                             ICacheStore<string> fixtureTimestampCacheStore)
            : base(id, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCacheStore)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionCacheItem"/> class
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixtureDto</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="ICacheStore{T}"/> used to cache the sport events fixtureDto timestamps</param>
        public CompetitionCacheItem(CompetitionDto eventSummary,
                             IDataRouterManager dataRouterManager,
                             ISemaphorePool semaphorePool,
                             CultureInfo currentCulture,
                             CultureInfo defaultCulture,
                             ICacheStore<string> fixtureTimestampCache)
            : base(eventSummary, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Guard.Argument(eventSummary, nameof(eventSummary)).NotNull();
            Guard.Argument(currentCulture, nameof(currentCulture)).NotNull();

            Merge(eventSummary, currentCulture, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionCacheItem"/> class
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixtureDto</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="ICacheStore{T}"/> used to cache the sport events fixtureDto timestamps</param>
        public CompetitionCacheItem(TournamentInfoDto eventSummary,
                            IDataRouterManager dataRouterManager,
                            ISemaphorePool semaphorePool,
                            CultureInfo currentCulture,
                            CultureInfo defaultCulture,
                            ICacheStore<string> fixtureTimestampCache)
            : base(eventSummary, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Guard.Argument(eventSummary, nameof(eventSummary)).NotNull();
            Guard.Argument(currentCulture, nameof(currentCulture)).NotNull();

            Merge(eventSummary, currentCulture, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionCacheItem" /> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableSportEvent" /> representing the sport event</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixtureDto</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCacheStore">A <see cref="ICacheStore{T}"/> used to cache the sport events fixtureDto timestamps</param>
        public CompetitionCacheItem(ExportableSportEvent exportable,
            IDataRouterManager dataRouterManager,
            ISemaphorePool semaphorePool,
            CultureInfo defaultCulture,
            ICacheStore<string> fixtureTimestampCacheStore)
            : base(exportable, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCacheStore)
        {
            Guard.Argument(exportable, nameof(exportable)).NotNull();

            if (exportable is ExportableCompetition exportableCompetition)
            {
                _bookingStatus = exportableCompetition.BookingStatus;
                _venue = exportableCompetition.Venue != null ? new VenueCacheItem(exportableCompetition.Venue) : null;
                _conditions = exportableCompetition.Conditions != null
                    ? new SportEventConditionsCacheItem(exportableCompetition.Conditions)
                    : null;
                Competitors = exportableCompetition.Competitors != null
                    ? new List<Urn>(exportableCompetition.Competitors.Select(Urn.Parse))
                    : null;
                _referenceId = exportableCompetition.ReferenceId != null
                    ? new ReferenceIdCacheItem(exportableCompetition.ReferenceId)
                    : null;
                _competitorsQualifiers = exportableCompetition.CompetitorsQualifiers != null
                    ? new Dictionary<Urn, string>(
                        exportableCompetition.CompetitorsQualifiers.ToDictionary(c => Urn.Parse(c.Key), c => c.Value))
                    : null;
                _competitorsReferences = exportableCompetition.CompetitorsReferences != null
                    ? new Dictionary<Urn, ReferenceIdCacheItem>(
                        exportableCompetition.CompetitorsReferences.ToDictionary(c => Urn.Parse(c.Key),
                            c => new ReferenceIdCacheItem(c.Value)))
                    : null;
                _liveOdds = string.IsNullOrEmpty(exportableCompetition.LiveOdds) ? null : exportableCompetition.LiveOdds;
                _sportEventType = exportableCompetition.SportEventType;
                _stageType = exportableCompetition.StageType;
            }
        }

        /// <summary>
        /// Asynchronously fetch event summary associated with the current instance (saving done in <see cref="ISportEventStatusCache"/>)
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<bool> FetchSportEventStatusAsync()
        {
            await FetchMissingSummary(new[] { DefaultCulture }, true).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="BookingStatus"/> enum member providing booking status for the associated entity or a null reference if booking status is not known
        /// </summary>
        /// <returns>Asynchronously returns the <see cref="BookingStatus"/> if available</returns>
        public async Task<BookingStatus?> GetBookingStatusAsync()
        {
            if (_bookingStatus != null || LoadedFixtures.Any())
            {
                return _bookingStatus;
            }
            await FetchMissingFixtures(new[] { DefaultCulture }).ConfigureAwait(false);
            return _bookingStatus;
        }

        /// <summary>
        /// get venue as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        public async Task<VenueCacheItem> GetVenueAsync(IEnumerable<CultureInfo> cultures)
        {
            var cultureInfos = cultures as CultureInfo[] ?? cultures.ToArray();
            if (_venue != null && _venue.HasTranslationsFor(cultureInfos))
            {
                return _venue;
            }
            await FetchMissingSummary(cultureInfos, false).ConfigureAwait(false);

            if (_venue == null)
            {
                await FetchMissingFixtures(cultureInfos).ConfigureAwait(false);
            }

            return _venue;
        }

        /// <summary>
        /// get conditions as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        public async Task<SportEventConditionsCacheItem> GetConditionsAsync(IEnumerable<CultureInfo> cultures)
        {
            var cultureInfos = cultures as CultureInfo[] ?? cultures.ToArray();
            if (_conditions?.Referee != null && _conditions.Referee.HasTranslationsFor(cultureInfos))
            {
                return _conditions;
            }
            await FetchMissingSummary(cultureInfos, false).ConfigureAwait(false);
            return _conditions;
        }

        /// <summary>
        /// get competitors as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        public async Task<IEnumerable<Urn>> GetCompetitorsIdsAsync(IEnumerable<CultureInfo> cultures)
        {
            var cultureInfos = cultures.ToList();
            if (Competitors != null && Competitors.Any() && HasTranslationsFor(cultureInfos))
            {
                return Competitors;
            }
            await FetchMissingSummary(cultureInfos, false).ConfigureAwait(false);
            return Competitors;
        }

        /// <summary>
        /// get reference ids as an asynchronous operation
        /// </summary>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        public async Task<ReferenceIdCacheItem> GetReferenceIdsAsync()
        {
            if (_referenceId != null)
            {
                return _referenceId;
            }
            await FetchMissingFixtures(new[] { DefaultCulture }).ConfigureAwait(false);
            return _referenceId;
        }

        /// <summary>
        /// Asynchronously get the list of available team <see cref="ReferenceIdCacheItem"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IDictionary<Urn, ReferenceIdCacheItem>> GetCompetitorsReferencesAsync()
        {
            if (!LoadedFixtures.Any())
            {
                await FetchMissingFixtures(new[] { DefaultCulture }).ConfigureAwait(false);
            }
            return _competitorsReferences;
        }

        /// <summary>
        /// Asynchronously get the list of available team qualifiers
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IDictionary<Urn, string>> GetCompetitorsQualifiersAsync()
        {
            if (!LoadedSummaries.Any())
            {
                await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            }
            return _competitorsQualifiers;
        }

        /// <summary>
        /// Asynchronously gets a liveOdds
        /// </summary>
        /// <returns>A liveOdds</returns>
        public async Task<string> GetLiveOddsAsync()
        {
            if (!string.IsNullOrEmpty(_liveOdds))
            {
                return _liveOdds;
            }
            if (LoadedSummaries.Any())
            {
                return _liveOdds;
            }
            await FetchMissingSummary(new List<CultureInfo> { DefaultCulture }, false).ConfigureAwait(false);
            return _liveOdds;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="SportEventType"/> for the associated sport event.
        /// </summary>
        /// <returns>A <see cref="SportEventType"/> for the associated sport event.</returns>
        public async Task<SportEventType?> GetSportEventTypeAsync()
        {
            if (_sportEventType != null)
            {
                return _sportEventType;
            }
            if (LoadedSummaries.Any())
            {
                return _sportEventType;
            }
            await FetchMissingSummary(new List<CultureInfo> { DefaultCulture }, false).ConfigureAwait(false);
            return _sportEventType;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="StageType"/> for the associated sport event.
        /// </summary>
        /// <returns>A <see cref="StageType"/> for the associated sport event.</returns>
        public async Task<StageType?> GetStageTypeAsync()
        {
            if (_stageType != null)
            {
                return _stageType;
            }
            if (LoadedSummaries.Any())
            {
                return _stageType;
            }
            await FetchMissingSummary(new List<CultureInfo> { DefaultCulture }, false).ConfigureAwait(false);
            return _stageType;
        }

        /// <summary>
        /// Merges the specified event summary
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void Merge(CompetitionDto eventSummary, CultureInfo culture, bool useLock)
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
        private void ActualMerge(CompetitionDto eventSummary, CultureInfo culture)
        {
            base.Merge(eventSummary, culture, false);

            if (eventSummary.Venue != null)
            {
                if (_venue == null)
                {
                    _venue = new VenueCacheItem(eventSummary.Venue, culture);
                }
                else
                {
                    _venue.Merge(eventSummary.Venue, culture);
                }
            }
            if (eventSummary.Conditions != null)
            {
                if (_conditions == null)
                {
                    _conditions = new SportEventConditionsCacheItem(eventSummary.Conditions, culture);
                }
                else
                {
                    _conditions.Merge(eventSummary.Conditions, culture);
                }
            }
            if (eventSummary.Competitors != null)
            {
                Competitors = new List<Urn>(eventSummary.Competitors.Select(t => t.Id));
                GenerateMatchName(eventSummary.Competitors, culture);
                FillCompetitorsQualifiers(eventSummary.Competitors);
                FillCompetitorsReferences(eventSummary.Competitors);
            }
            if (eventSummary.BookingStatus != null)
            {
                _bookingStatus = eventSummary.BookingStatus;
            }
            if (!string.IsNullOrEmpty(eventSummary.LiveOdds))
            {
                _liveOdds = eventSummary.LiveOdds;
            }
            if (eventSummary.Type != null)
            {
                _sportEventType = eventSummary.Type;
            }
            if (eventSummary.StageType != null)
            {
                _stageType = eventSummary.StageType;
            }
        }

        /// <summary>
        /// Merges the specified fixtureDto
        /// </summary>
        /// <param name="fixtureDto">The fixtureDto</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void MergeFixture(FixtureDto fixtureDto, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    if (fixtureDto.ReferenceIds != null)
                    {
                        _referenceId = new ReferenceIdCacheItem(fixtureDto.ReferenceIds);
                    }
                    if (fixtureDto.BookingStatus != null)
                    {
                        _bookingStatus = fixtureDto.BookingStatus;
                    }
                }
            }
            else
            {
                if (fixtureDto.ReferenceIds != null)
                {
                    _referenceId = new ReferenceIdCacheItem(fixtureDto.ReferenceIds);
                }
                if (fixtureDto.BookingStatus != null)
                {
                    _bookingStatus = fixtureDto.BookingStatus;
                }
            }

            if (!string.IsNullOrEmpty(fixtureDto.LiveOdds))
            {
                _liveOdds = fixtureDto.LiveOdds;
            }

            if (fixtureDto.Type != null)
            {
                _sportEventType = fixtureDto.Type;
            }

            if (fixtureDto.StageType != null)
            {
                _stageType = fixtureDto.StageType;
            }

            if (fixtureDto.Venue != null)
            {
                if (_venue == null)
                {
                    _venue = new VenueCacheItem(fixtureDto.Venue, culture);
                }
                else
                {
                    _venue.Merge(fixtureDto.Venue, culture);
                }
            }
        }

        private void GenerateMatchName(IEnumerable<TeamCompetitorDto> competitors, CultureInfo culture)
        {
            var teamCompetitorDtos = competitors.ToList();
            if (Id.TypeGroup == ResourceTypeGroup.Match && teamCompetitorDtos.Count == 2)
            {
                if (Names.TryGetValue(culture, out var name) && !string.IsNullOrEmpty(name))
                {
                    return;
                }

                var homeTeam = teamCompetitorDtos.FirstOrDefault(f => f.Qualifier == "home");
                if (homeTeam == null)
                {
                    return;
                }
                var awayTeam = teamCompetitorDtos.FirstOrDefault(f => f.Qualifier == "away");
                if (awayTeam == null)
                {
                    return;
                }
                Names[culture] = homeTeam.Name + " vs. " + awayTeam.Name;
            }
        }

        private void FillCompetitorsQualifiers(IEnumerable<TeamCompetitorDto> competitors)
        {
            if (competitors == null)
            {
                return;
            }
            if (_competitorsQualifiers == null)
            {
                _competitorsQualifiers = new Dictionary<Urn, string>();
            }
            foreach (var competitor in competitors)
            {
                if (!string.IsNullOrEmpty(competitor.Qualifier))
                {
                    _competitorsQualifiers[competitor.Id] = competitor.Qualifier;
                }
            }
        }

        private void FillCompetitorsReferences(IEnumerable<TeamCompetitorDto> competitors)
        {
            if (competitors == null)
            {
                return;
            }
            if (_competitorsReferences == null)
            {
                _competitorsReferences = new Dictionary<Urn, ReferenceIdCacheItem>();
            }
            foreach (var competitor in competitors)
            {
                if (competitor.ReferenceIds != null && competitor.ReferenceIds.Any())
                {
                    if (_competitorsReferences.ContainsKey(competitor.Id))
                    {
                        var compRefs = _competitorsReferences[competitor.Id];
                        compRefs.Merge(competitor.ReferenceIds, true);
                    }
                    else
                    {
                        _competitorsReferences[competitor.Id] = new ReferenceIdCacheItem(competitor.ReferenceIds);
                    }
                }
            }
        }

        /// <summary>
        /// Change booking status to Booked
        /// </summary>
        public void Book()
        {
            _bookingStatus = BookingStatus.Booked;
        }

        protected override async Task<T> CreateExportableBaseAsync<T>()
        {
            var exportable = await base.CreateExportableBaseAsync<T>();

            if (exportable is ExportableCompetition competition)
            {
                competition.BookingStatus = _bookingStatus;
                competition.Venue = _venue != null ? await _venue.ExportAsync() : null;
                competition.Conditions = _conditions != null ? await _conditions.ExportAsync() : null;
                competition.Competitors = Competitors?.Select(c => c.ToString()).ToList();
                competition.ReferenceId = _referenceId?.ReferenceIds?.ToDictionary(r => r.Key, r => r.Value);
                competition.CompetitorsQualifiers = _competitorsQualifiers?.ToDictionary(q => q.Key.ToString(), q => q.Value);
                competition.CompetitorsReferences = _competitorsReferences?.ToDictionary(r => r.Key.ToString(), r => (IDictionary<string, string>)r.Value.ReferenceIds.ToDictionary(v => v.Key, v => v.Value));
#pragma warning disable CS0618 // Type or member is obsolete
                competition.CompetitorsVirtual = null; // later this property should be removed
#pragma warning restore CS0618 // Type or member is obsolete
                competition.LiveOdds = _liveOdds;
                competition.SportEventType = _sportEventType;
                competition.StageType = _stageType;
            }

            return exportable;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public override async Task<ExportableBase> ExportAsync()
        {
            return await CreateExportableBaseAsync<ExportableCompetition>().ConfigureAwait(false);
        }
    }
}
