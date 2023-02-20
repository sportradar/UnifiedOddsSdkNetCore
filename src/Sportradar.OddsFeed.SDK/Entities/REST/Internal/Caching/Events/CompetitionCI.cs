/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Dawn;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Class CompetitionCI
    /// </summary>
    /// <seealso cref="ISportEventCI" />
    /// <seealso cref="ICompetitionCI" />
    internal class CompetitionCI : SportEventCI, ICompetitionCI
    {
        /// <summary>
        /// The booking status
        /// </summary>
        private BookingStatus? _bookingStatus;
        /// <summary>
        /// The venue
        /// </summary>
        private VenueCI _venue;
        /// <summary>
        /// The conditions
        /// </summary>
        private SportEventConditionsCI _conditions;
        /// <summary>
        /// The competitors
        /// </summary>
        protected IEnumerable<URN> Competitors;
        /// <summary>
        /// The reference identifier
        /// </summary>
        private ReferenceIdCI _referenceId;
        /// <summary>
        /// The competitors qualifiers
        /// </summary>
        private IDictionary<URN, string> _competitorsQualifiers;
        /// <summary>
        /// The competitors references
        /// </summary>
        private IDictionary<URN, ReferenceIdCI> _competitorsReferences;
        /// <summary>
        /// The competitors isVirtual attribute
        /// </summary>
        private IList<URN> _competitorsVirtual;

        private string _liveOdds;

        private SportEventType? _sportEventType;

        private StageType? _stageType;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionCI"/> class
        /// </summary>
        /// <param name="id">A <see cref="URN" /> specifying the id of the sport event associated with the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixtureDTO</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ...)</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixtureDTO timestamps</param>
        public CompetitionCI(URN id,
                             IDataRouterManager dataRouterManager,
                             ISemaphorePool semaphorePool,
                             CultureInfo defaultCulture,
                             MemoryCache fixtureTimestampCache)
            : base(id, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionCI"/> class
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixtureDTO</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixtureDTO timestamps</param>
        public CompetitionCI(CompetitionDTO eventSummary,
                             IDataRouterManager dataRouterManager,
                             ISemaphorePool semaphorePool,
                             CultureInfo currentCulture,
                             CultureInfo defaultCulture,
                             MemoryCache fixtureTimestampCache)
            : base(eventSummary, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Guard.Argument(eventSummary, nameof(eventSummary)).NotNull();
            Guard.Argument(currentCulture, nameof(currentCulture)).NotNull();

            Merge(eventSummary, currentCulture, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionCI"/> class
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixtureDTO</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixtureDTO timestamps</param>
        public CompetitionCI(TournamentInfoDTO eventSummary,
                            IDataRouterManager dataRouterManager,
                            ISemaphorePool semaphorePool,
                            CultureInfo currentCulture,
                            CultureInfo defaultCulture,
                            MemoryCache fixtureTimestampCache)
            : base(eventSummary, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Guard.Argument(eventSummary, nameof(eventSummary)).NotNull();
            Guard.Argument(currentCulture, nameof(currentCulture)).NotNull();

            Merge(eventSummary, currentCulture, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitionCI" /> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableSportEventCI" /> representing the sport event</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixtureDTO</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixtureDTO timestamps</param>
        public CompetitionCI(ExportableSportEventCI exportable,
            IDataRouterManager dataRouterManager,
            ISemaphorePool semaphorePool,
            CultureInfo defaultCulture,
            MemoryCache fixtureTimestampCache)
            : base(exportable, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
            Guard.Argument(exportable, nameof(exportable)).NotNull();

            if (exportable is ExportableCompetitionCI exportableCompetition)
            {
                _bookingStatus = exportableCompetition.BookingStatus;
                _venue = exportableCompetition.Venue != null ? new VenueCI(exportableCompetition.Venue) : null;
                _conditions = exportableCompetition.Conditions != null
                    ? new SportEventConditionsCI(exportableCompetition.Conditions)
                    : null;
                Competitors = exportableCompetition.Competitors != null
                    ? new List<URN>(exportableCompetition.Competitors.Select(URN.Parse))
                    : null;
                _referenceId = exportableCompetition.ReferenceId != null
                    ? new ReferenceIdCI(exportableCompetition.ReferenceId)
                    : null;
                _competitorsQualifiers = exportableCompetition.CompetitorsQualifiers != null
                    ? new Dictionary<URN, string>(
                        exportableCompetition.CompetitorsQualifiers.ToDictionary(c => URN.Parse(c.Key), c => c.Value))
                    : null;
                _competitorsReferences = exportableCompetition.CompetitorsReferences != null
                    ? new Dictionary<URN, ReferenceIdCI>(
                        exportableCompetition.CompetitorsReferences.ToDictionary(c => URN.Parse(c.Key),
                            c => new ReferenceIdCI(c.Value)))
                    : null;
                _competitorsVirtual = exportableCompetition.CompetitorsVirtual != null
                                          ? exportableCompetition.CompetitorsVirtual.Select(URN.Parse).ToList()
                                          : new List<URN>();
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
        public async Task<VenueCI> GetVenueAsync(IEnumerable<CultureInfo> cultures)
        {
            var cultureInfos = cultures as CultureInfo[] ?? cultures.ToArray();
            if (_venue != null && _venue.HasTranslationsFor(cultureInfos))
            {
                return _venue;
            }
            await FetchMissingSummary(cultureInfos, false).ConfigureAwait(false);
            return _venue;
        }

        /// <summary>
        /// get conditions as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        public async Task<SportEventConditionsCI> GetConditionsAsync(IEnumerable<CultureInfo> cultures)
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
        public async Task<IEnumerable<URN>> GetCompetitorsIdsAsync(IEnumerable<CultureInfo> cultures)
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
        public async Task<ReferenceIdCI> GetReferenceIdsAsync()
        {
            if (_referenceId != null)
            {
                return _referenceId;
            }
            await FetchMissingFixtures(new[] { DefaultCulture }).ConfigureAwait(false);
            return _referenceId;
        }

        /// <summary>
        /// Asynchronously get the list of available team <see cref="ReferenceIdCI"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IDictionary<URN, ReferenceIdCI>> GetCompetitorsReferencesAsync()
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
        public async Task<IDictionary<URN, string>> GetCompetitorsQualifiersAsync()
        {
            if (!LoadedSummaries.Any())
            {
                await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            }
            return _competitorsQualifiers;
        }

        /// <summary>
        /// Asynchronously get the list of competitors marked as virtual
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IList<URN>> GetCompetitorsVirtualAsync()
        {
            if (!LoadedSummaries.Any())
            {
                await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            }
            return _competitorsVirtual;
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
        public void Merge(CompetitionDTO eventSummary, CultureInfo culture, bool useLock)
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
        private void ActualMerge(CompetitionDTO eventSummary, CultureInfo culture)
        {
            base.Merge(eventSummary, culture, false);

            if (eventSummary.Venue != null)
            {
                if (_venue == null)
                {
                    _venue = new VenueCI(eventSummary.Venue, culture);
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
                    _conditions = new SportEventConditionsCI(eventSummary.Conditions, culture);
                }
                else
                {
                    _conditions.Merge(eventSummary.Conditions, culture);
                }
            }
            if (eventSummary.Competitors != null)
            {
                Competitors = new List<URN>(eventSummary.Competitors.Select(t => t.Id));
                GenerateMatchName(eventSummary.Competitors, culture);
                FillCompetitorsQualifiers(eventSummary.Competitors);
                FillCompetitorsReferences(eventSummary.Competitors);
                FillCompetitorsVirtual(eventSummary.Competitors);
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
        /// Merges the specified fixtureDTO
        /// </summary>
        /// <param name="fixtureDTO">The fixtureDTO</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void MergeFixture(FixtureDTO fixtureDTO, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    if (fixtureDTO.ReferenceIds != null)
                    {
                        _referenceId = new ReferenceIdCI(fixtureDTO.ReferenceIds);
                    }
                    if (fixtureDTO.BookingStatus != null)
                    {
                        _bookingStatus = fixtureDTO.BookingStatus;
                    }
                }
            }
            else
            {
                if (fixtureDTO.ReferenceIds != null)
                {
                    _referenceId = new ReferenceIdCI(fixtureDTO.ReferenceIds);
                }
                if (fixtureDTO.BookingStatus != null)
                {
                    _bookingStatus = fixtureDTO.BookingStatus;
                }
            }

            if (!string.IsNullOrEmpty(fixtureDTO.LiveOdds))
            {
                _liveOdds = fixtureDTO.LiveOdds;
            }

            if (fixtureDTO.Type != null)
            {
                _sportEventType = fixtureDTO.Type;
            }

            if (fixtureDTO.StageType != null)
            {
                _stageType = fixtureDTO.StageType;
            }
        }

        private void GenerateMatchName(IEnumerable<TeamCompetitorDTO> competitors, CultureInfo culture)
        {
            var teamCompetitorDtos = competitors.ToList();
            if (Id.TypeGroup == ResourceTypeGroup.MATCH && teamCompetitorDtos.Count == 2)
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

        private void FillCompetitorsQualifiers(IEnumerable<TeamCompetitorDTO> competitors)
        {
            if (competitors == null)
            {
                return;
            }
            if (_competitorsQualifiers == null)
            {
                _competitorsQualifiers = new Dictionary<URN, string>();
            }
            foreach (var competitor in competitors)
            {
                if (!string.IsNullOrEmpty(competitor.Qualifier))
                {
                    _competitorsQualifiers[competitor.Id] = competitor.Qualifier;
                }
            }
        }

        private void FillCompetitorsReferences(IEnumerable<TeamCompetitorDTO> competitors)
        {
            if (competitors == null)
            {
                return;
            }
            if (_competitorsReferences == null)
            {
                _competitorsReferences = new Dictionary<URN, ReferenceIdCI>();
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
                        _competitorsReferences[competitor.Id] = new ReferenceIdCI(competitor.ReferenceIds);
                    }
                }
            }
        }

        private void FillCompetitorsVirtual(IEnumerable<TeamCompetitorDTO> competitors)
        {
            if (competitors == null)
            {
                return;
            }
            if (_competitorsVirtual == null)
            {
                _competitorsVirtual = new List<URN>();
            }
            foreach (var competitor in competitors)
            {
                if (competitor.IsVirtual && !_competitorsVirtual.Contains(competitor.Id))
                {
                    _competitorsVirtual.Add(competitor.Id);
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

        protected override async Task<T> CreateExportableCIAsync<T>()
        {
            var exportable = await base.CreateExportableCIAsync<T>();

            if (exportable is ExportableCompetitionCI competition)
            {
                competition.BookingStatus = _bookingStatus;
                competition.Venue = _venue != null ? await _venue.ExportAsync() : null;
                competition.Conditions = _conditions != null ? await _conditions.ExportAsync() : null;
                competition.Competitors = Competitors?.Select(c => c.ToString()).ToList();
                competition.ReferenceId = _referenceId?.ReferenceIds?.ToDictionary(r => r.Key, r => r.Value);
                competition.CompetitorsQualifiers = _competitorsQualifiers?.ToDictionary(q => q.Key.ToString(), q => q.Value);
                competition.CompetitorsReferences = _competitorsReferences?.ToDictionary(r => r.Key.ToString(), r => (IDictionary<string, string>)r.Value.ReferenceIds.ToDictionary(v => v.Key, v => v.Value));
                competition.CompetitorsVirtual = _competitorsVirtual.IsNullOrEmpty() ? null : _competitorsVirtual.Select(s => s.ToString()).ToList();
                competition.LiveOdds = _liveOdds;
                competition.SportEventType = _sportEventType;
                competition.StageType = _stageType;
            }

            return exportable;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public override async Task<ExportableCI> ExportAsync() => await CreateExportableCIAsync<ExportableCompetitionCI>().ConfigureAwait(false);
    }
}
