﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dawn;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Class TournamentInfoCI
    /// </summary>
    /// <seealso cref="SportEventCI" />
    /// <seealso cref="ITournamentInfoCI" />
    internal class TournamentInfoCI : SportEventCI, ITournamentInfoCI
    {
        /// <summary>
        /// The category identifier
        /// </summary>
        private URN _categoryId;
        /// <summary>
        /// The tournament coverage
        /// </summary>
        private TournamentCoverageCI _tournamentCoverage;
        /// <summary>
        /// The competitors ids
        /// </summary>
        private IEnumerable<URN> _competitors;
        /// <summary>
        /// The current season information
        /// </summary>
        private CurrentSeasonInfoCI _currentSeasonInfo;
        /// <summary>
        /// The groups
        /// </summary>
        private IEnumerable<GroupCI> _groups;
        /// <summary>
        /// The schedule urns
        /// </summary>
        private IEnumerable<URN> _scheduleUrns;
        /// <summary>
        /// The round
        /// </summary>
        private RoundCI _round;
        /// <summary>
        /// The year
        /// </summary>
        private string _year;
        /// <summary>
        /// The tournament information basic
        /// </summary>
        private TournamentInfoBasicCI _tournamentInfoBasic;
        /// <summary>
        /// The reference identifier
        /// </summary>
        private ReferenceIdCI _referenceId;
        /// <summary>
        /// The season coverage
        /// </summary>
        private SeasonCoverageCI _seasonCoverage;
        /// <summary>
        /// The seasons
        /// </summary>
        private IEnumerable<URN> _seasons;

        /// <summary>
        /// The loaded seasons for tournament
        /// </summary>
        private readonly List<CultureInfo> _loadedSeasons = new List<CultureInfo>();

        /// <summary>
        /// The loaded schedules for tournament
        /// </summary>
        private readonly List<CultureInfo> _loadedSchedules = new List<CultureInfo>();

        /// <summary>
        /// The competitors references
        /// </summary>
        private IDictionary<URN, ReferenceIdCI> _competitorsReferences;

        /// <summary>
        /// The indicator specifying if the tournament is exhibition game
        /// </summary>
        private bool? _exhibitionGames;

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentInfoCI"/> class
        /// </summary>
        /// <param name="id">A <see cref="URN" /> specifying the id of the sport event associated with the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixture timestamps</param>
        public TournamentInfoCI(URN id,
                                IDataRouterManager dataRouterManager,
                                ISemaphorePool semaphorePool,
                                CultureInfo defaultCulture,
                                MemoryCache fixtureTimestampCache)
            : base(id, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentInfoCI"/> class
        /// </summary>
        /// <param name="eventSummary">The sport event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="currentCulture">A <see cref="CultureInfo" /> of the <see cref="SportEventSummaryDTO" /> instance</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixture timestamps</param>
        public TournamentInfoCI(TournamentInfoDTO eventSummary,
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
        /// Initializes a new instance of the <see cref="TournamentInfoCI"/> class
        /// </summary>
        /// <param name="fixture">The fixture data</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="currentCulture">A <see cref="CultureInfo" /> of the <see cref="SportEventSummaryDTO" /> instance</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixture timestamps</param>
        public TournamentInfoCI(FixtureDTO fixture,
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
        /// Initializes a new instance of the <see cref="TournamentInfoCI"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableTournamentInfoCI" /> specifying the current instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool" /> instance used to obtain sync objects</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo" /> specifying the language used when fetching info which is not translatable (e.g. Scheduled, ..)</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixture timestamps</param>
        public TournamentInfoCI(ExportableTournamentInfoCI exportable,
            IDataRouterManager dataRouterManager,
            ISemaphorePool semaphorePool,
            CultureInfo defaultCulture,
            MemoryCache fixtureTimestampCache)
            : base(exportable, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
            _categoryId = exportable.CategoryId == null ? null : URN.Parse(exportable.CategoryId);
            _tournamentCoverage = exportable.TournamentCoverage != null
                ? new TournamentCoverageCI(exportable.TournamentCoverage)
                : null;
            _competitors = exportable.Competitors?.Select(URN.Parse).ToList();
            _currentSeasonInfo = exportable.CurrentSeasonInfo != null
                ? new CurrentSeasonInfoCI(exportable.CurrentSeasonInfo)
                : null;
            _groups = exportable.Groups?.Select(g => new GroupCI(g)).ToList();
            _scheduleUrns = exportable.ScheduleUrns?.Select(URN.Parse).ToList();
            _round = exportable.Round != null ? new RoundCI(exportable.Round) : null;
            _year = exportable.Year;
            _tournamentInfoBasic = exportable.TournamentInfoBasic != null
                ? new TournamentInfoBasicCI(exportable.TournamentInfoBasic)
                : null;
            _referenceId = exportable.ReferenceId != null ? new ReferenceIdCI(exportable.ReferenceId) : null;
            _seasonCoverage = exportable.SeasonCoverage != null
                ? new SeasonCoverageCI(exportable.SeasonCoverage)
                : null;
            _seasons = exportable.Seasons?.Select(URN.Parse).ToList();
            _loadedSeasons = new List<CultureInfo>(exportable.LoadedSeasons ?? new List<CultureInfo>());
            _loadedSchedules = new List<CultureInfo>(exportable.LoadedSchedules ?? new List<CultureInfo>());
            _competitorsReferences =
                exportable.CompetitorsReferences?.ToDictionary(c => URN.Parse(c.Key), c => new ReferenceIdCI(c.Value));
            _exhibitionGames = exportable.ExhibitionGames;
        }

        /// <summary>
    /// Get category identifier as an asynchronous operation
    /// </summary>
    /// <returns>A <see cref="Task{URN}" /> representing the asynchronous operation</returns>
    public async Task<URN> GetCategoryIdAsync()
        {
            if (_categoryId != null)
            {
                return _categoryId;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _categoryId;
        }

        /// <summary>
        /// Get tournament coverage as an asynchronous operation
        /// </summary>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<TournamentCoverageCI> GetTournamentCoverageAsync()
        {
            if (_tournamentCoverage != null)
            {
                return _tournamentCoverage;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _tournamentCoverage;
        }

        /// <summary>
        /// Get competitors ids as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<IEnumerable<URN>> GetCompetitorsIdsAsync(IEnumerable<CultureInfo> cultures)
        {
            var wantedCultures = cultures as List<CultureInfo> ?? cultures.ToList();
            wantedCultures = LanguageHelper.GetMissingCultures(wantedCultures, LoadedSummaries).ToList();
            if (_competitors != null && !wantedCultures.Any())
            {
                return await PrepareCompetitorList(_competitors, wantedCultures);
            }
            await FetchMissingSummary(wantedCultures, false).ConfigureAwait(false);
            return await PrepareCompetitorList(_competitors, wantedCultures);
        }

        private async Task<IEnumerable<URN>> PrepareCompetitorList(IEnumerable<URN> competitors, IEnumerable<CultureInfo> cultures)
        {
            if (competitors != null)
            {
                return competitors;
            }

            var groups = await GetGroupsAsync(cultures);
            return groups?.SelectMany(g => g.CompetitorsIds).Distinct();
        }

        /// <summary>
        /// Get current season information as an asynchronous operation
        /// </summary>
        /// <param name="cultures">The cultures</param>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<CurrentSeasonInfoCI> GetCurrentSeasonInfoAsync(IEnumerable<CultureInfo> cultures)
        {
            var wantedCultures = cultures as CultureInfo[] ?? cultures.ToArray();
            if (_currentSeasonInfo != null && _currentSeasonInfo.HasTranslationsFor(wantedCultures))
            {
                return _currentSeasonInfo;
            }
            await FetchMissingSummary(wantedCultures, false).ConfigureAwait(false);
            return _currentSeasonInfo;
        }

        /// <summary>
        /// Get groups as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<IEnumerable<GroupCI>> GetGroupsAsync(IEnumerable<CultureInfo> cultures)
        {
            var wantedCultures = cultures as CultureInfo[] ?? cultures.ToArray();
            if (_groups != null && !LanguageHelper.GetMissingCultures(wantedCultures, LoadedSummaries).Any())
            {
                return _groups;
            }
            await FetchMissingSummary(wantedCultures, false).ConfigureAwait(false);
            return _groups;
        }

        /// <summary>
        /// Get schedule as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<IEnumerable<URN>> GetScheduleAsync(IEnumerable<CultureInfo> cultures)
        {
            var missingCultures = LanguageHelper.GetMissingCultures(cultures, _loadedSchedules).ToList();
            if (_scheduleUrns == null && missingCultures.Any())
            {
                var tasks = missingCultures.Select(s => DataRouterManager.GetSportEventsForTournamentAsync(Id, s, this)).ToList();
                await Task.WhenAll(tasks).ConfigureAwait(false);

                if (tasks.All(a => a.IsCompleted))
                {
                    _loadedSchedules.AddRange(missingCultures);
                    if (tasks.First().Result != null)
                    {
                        _scheduleUrns = tasks.First().Result.Select(s => s.Item1);
                    }
                }
            }

            if (_scheduleUrns.IsNullOrEmpty() && _currentSeasonInfo != null)
            {
                _scheduleUrns = _currentSeasonInfo.Schedule;
            }

            return _scheduleUrns;
        }

        /// <summary>
        /// Get current round as an asynchronous operation
        /// </summary>
        /// <param name="cultures">The cultures</param>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<RoundCI> GetCurrentRoundAsync(IEnumerable<CultureInfo> cultures)
        {
            var wantedCultures = cultures as CultureInfo[] ?? cultures.ToArray();
            if (_round != null && _round.HasTranslationsFor(wantedCultures))
            {
                return _round;
            }
            await FetchMissingSummary(wantedCultures, false).ConfigureAwait(false);
            return _round;
        }

        /// <summary>
        /// Get year as an asynchronous operation
        /// </summary>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<string> GetYearAsync()
        {
            if (!string.IsNullOrEmpty(_year))
            {
                return _year;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _year;
        }

        /// <summary>
        /// Get tournament information as an asynchronous operation
        /// </summary>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<TournamentInfoBasicCI> GetTournamentInfoAsync()
        {
            if (_tournamentInfoBasic != null)
            {
                return _tournamentInfoBasic;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _tournamentInfoBasic;
        }

        /// <summary>
        /// Get reference ids as an asynchronous operation
        /// </summary>
        /// <returns>A <see cref="Task{T}" /> representing an async operation</returns>
        public async Task<ReferenceIdCI> GetReferenceIdsAsync()
        {
            if (_referenceId != null)
            {
                return _referenceId;
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _referenceId;
        }

        /// <summary>
        /// Get season coverage as an asynchronous operation
        /// </summary>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<SeasonCoverageCI> GetSeasonCoverageAsync()
        {
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _seasonCoverage;
        }

        /// <summary>
        /// Get seasons as an asynchronous operation
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{TResult}" /> representing an async operation</returns>
        public async Task<IEnumerable<URN>> GetSeasonsAsync(IEnumerable<CultureInfo> cultures)
        {
            var missingCultures = LanguageHelper.GetMissingCultures(cultures, _loadedSeasons).ToList();
            if (_seasons == null && missingCultures.Any())
            {
                var tasks = missingCultures.Select(s => DataRouterManager.GetSeasonsForTournamentAsync(Id, s, this)).ToList();
                await Task.WhenAll(tasks).ConfigureAwait(false);

                if (tasks.All(a => a.IsCompleted))
                {
                    _loadedSeasons.AddRange(missingCultures);
                    _seasons = tasks.First().Result;
                }
            }
            return _seasons;
        }

        /// <summary>
        /// Asynchronously get the list of available team <see cref="ReferenceIdCI"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public async Task<IDictionary<URN, ReferenceIdCI>> GetCompetitorsReferencesAsync()
        {
            if (_competitorsReferences != null)
            {
                return await PrepareCompetitorsReferences(_competitorsReferences);
            }
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return await PrepareCompetitorsReferences(_competitorsReferences);
        }

        private async Task<IDictionary<URN, ReferenceIdCI>> PrepareCompetitorsReferences(IDictionary<URN, ReferenceIdCI> competitorsReferences)
        {
            if (competitorsReferences != null)
            {
                return competitorsReferences;
            }

            var groups = await GetGroupsAsync(new[] { DefaultCulture });
            return groups?.SelectMany(g => g.CompetitorsReferences).Distinct().ToDictionary(c => c.Key, c => c.Value);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="bool"/> specifying if the tournament is exhibition game
        /// </summary>
        /// <returns>A <see cref="bool"/> specifying if the tournament is exhibition game</returns>
        public async Task<bool?> GetExhibitionGamesAsync()
        {
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _exhibitionGames;
        }

        /// <summary>
        /// Merges the specified dto
        /// </summary>
        /// <param name="dto">The dto</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">if set to <c>true</c> [use lock].</param>
        public void Merge(TournamentInfoDTO dto, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    ActualMerge(dto, culture);
                }
            }
            else
            {
                ActualMerge(dto, culture);
            }
        }

        /// <summary>
        /// Merges the specified dto
        /// </summary>
        /// <param name="dto">The dto</param>
        /// <param name="culture">The culture</param>
        private void ActualMerge(TournamentInfoDTO dto, CultureInfo culture)
        {
            base.Merge(dto, culture, false);

            if (dto.Category != null)
            {
                _categoryId = dto.Category.Id;
            }
            if (dto.TournamentCoverage != null)
            {
                _tournamentCoverage = new TournamentCoverageCI(dto.TournamentCoverage);
            }
            if (dto.Competitors != null)
            {
                if (_competitors == null)
                {
                    _competitors = new List<URN>(dto.Competitors.Select(t => t.Id));
                }
                else
                {
                    MergeCompetitors(dto.Competitors, culture);
                }
                FillCompetitorsReferences(dto.Competitors);
            }
            if (dto.CurrentSeason != null)
            {
                if (_currentSeasonInfo == null)
                {
                    _currentSeasonInfo = new CurrentSeasonInfoCI(dto.CurrentSeason, culture);
                }
                else
                {
                    _currentSeasonInfo.Merge(dto.CurrentSeason, culture);
                }
            }
            if (dto.Groups != null)
            {
                if (_groups == null)
                {
                    _groups = new List<GroupCI>(dto.Groups.Select(s => new GroupCI(s, culture)));
                }
                else
                {
                    MergeGroups(dto.Groups, culture);
                }
                var comps = new List<CompetitorDTO>();
                foreach (var groupDTO in dto.Groups)
                {
                    foreach (var groupDTOCompetitor in groupDTO.Competitors)
                    {
                        if (!comps.Any(c => c.Id.Equals(groupDTOCompetitor.Id)))
                        {
                            comps.Add(groupDTOCompetitor);
                        }
                    }
                }
                FillCompetitorsReferences(comps);
            }
            else if (_groups?.Count() > 0)
            {
                _groups = null;
            }
            if (dto.Schedule != null)
            {
                _scheduleUrns = new ReadOnlyCollection<URN>(dto.Schedule.Select(s => s.Id).ToList());
            }
            if (dto.CurrentRound != null)
            {
                if (_round == null)
                {
                    _round = new RoundCI(dto.CurrentRound, culture);
                }
                else
                {
                    _round.Merge(dto.CurrentRound, culture);
                }
            }
            if (!string.IsNullOrEmpty(dto.Year))
            {
                _year = dto.Year;
            }
            if (dto.TournamentInfo != null)
            {
                if (_tournamentInfoBasic == null)
                {
                    _tournamentInfoBasic = new TournamentInfoBasicCI(dto.TournamentInfo, culture);
                }
                else
                {
                    _tournamentInfoBasic.Merge(dto.TournamentInfo, culture);
                }
            }
            if (dto.SeasonCoverage != null)
            {
                _seasonCoverage = new SeasonCoverageCI(dto.SeasonCoverage);
            }

            if (dto.ExhibitionGames != null)
            {
                _exhibitionGames = dto.ExhibitionGames;
            }
        }

        /// <summary>
        /// Merges the specified fixture
        /// </summary>
        /// <param name="fixture">The fixture</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void MergeFixture(FixtureDTO fixture, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    ActualMergeFixture(fixture, culture);
                }
            }
            else
            {
                ActualMergeFixture(fixture, culture);
            }
        }

        /// <summary>
        /// Merges the specified fixture
        /// </summary>
        /// <param name="fixture">The fixture</param>
        /// <param name="culture">The culture</param>
        private void ActualMergeFixture(FixtureDTO fixture, CultureInfo culture)
        {
            Merge(new TournamentInfoDTO(fixture), culture, false);

            if (fixture.ReferenceIds != null)
            {
                _referenceId = new ReferenceIdCI(fixture.ReferenceIds);
            }
        }

        /// <summary>
        /// Merges the specified fixture
        /// </summary>
        /// <param name="tournamentSeasonsDTO">The <see cref="TournamentSeasonsDTO"/></param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void Merge(TournamentSeasonsDTO tournamentSeasonsDTO, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    Merge(tournamentSeasonsDTO.Tournament, culture, false);

                    if (tournamentSeasonsDTO.Seasons != null && tournamentSeasonsDTO.Seasons.Any())
                    {
                        _seasons = tournamentSeasonsDTO.Seasons.Select(s => s.Id);
                    }
                }
            }
            else
            {
                Merge(tournamentSeasonsDTO.Tournament, culture, false);

                if (tournamentSeasonsDTO.Seasons != null && tournamentSeasonsDTO.Seasons.Any())
                {
                    _seasons = tournamentSeasonsDTO.Seasons.Select(s => s.Id);
                }
            }
        }

        /// <summary>
        /// Merges the groups
        /// </summary>
        /// <param name="competitors">The groups</param>
        /// <param name="culture">The culture</param>
        private void MergeCompetitors(IEnumerable<CompetitorDTO> competitors, CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            if (competitors == null)
            {
                return;
            }

            var tempCompetitors = _competitors == null
                ? new List<URN>()
                : new List<URN>(_competitors);

            foreach (var competitor in competitors)
            {
                var tempCompetitor = tempCompetitors.FirstOrDefault(c => c.Equals(competitor.Id));
                if (tempCompetitor == null)
                {
                    tempCompetitors.Add(competitor.Id);
                }
            }
            _competitors = new ReadOnlyCollection<URN>(tempCompetitors);
        }

        /// <summary>
        /// Merges the groups
        /// </summary>
        /// <param name="groups">The groups</param>
        /// <param name="culture">The culture</param>
        private void MergeGroups(IEnumerable<GroupDTO> groups, CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            if (groups == null)
            {
                return;
            }

            var tmpGroups = _groups == null
                ? new List<GroupCI>()
                : new List<GroupCI>(_groups);

            var groupDTOs = groups.ToList();
            // remove obsolete groups
            if (!_groups.IsNullOrEmpty())
            {
                try
                {
                    if (groupDTOs.Count > 0 && !groupDTOs.Count.Equals(tmpGroups.Count))
                    {
                        tmpGroups.Clear();
                    }
                    else if(tmpGroups.Any(c => string.IsNullOrEmpty(c.Id) && string.IsNullOrEmpty(c.Name)) && !groupDTOs.Any(c => string.IsNullOrEmpty(c.Id) && string.IsNullOrEmpty(c.Name)))
                    {
                        tmpGroups.Clear();
                    }
                    else
                    {
                        foreach (var tmpGroup in _groups)
                        {
                            if (!string.IsNullOrEmpty(tmpGroup.Id))
                            {
                                if (groupDTOs.FirstOrDefault(f => f.Id.Equals(tmpGroup.Id, StringComparison.InvariantCultureIgnoreCase)) == null)
                                {
                                    tmpGroups.Remove(tmpGroup);
                                    continue;
                                }
                            }

                            if (string.IsNullOrEmpty(tmpGroup.Id) && !string.IsNullOrEmpty(tmpGroup.Name))
                            {
                                if (groupDTOs.FirstOrDefault(f => !string.IsNullOrEmpty(f.Name) && f.Name.Equals(tmpGroup.Name, StringComparison.InvariantCultureIgnoreCase)) == null)
                                {
                                    tmpGroups.Remove(tmpGroup);
                                    continue;
                                }
                            }

                            if (string.IsNullOrEmpty(tmpGroup.Id) && string.IsNullOrEmpty(tmpGroup.Name))
                            {
                                if (groupDTOs.First(f => string.IsNullOrEmpty(f.Id) && string.IsNullOrEmpty(f.Name)) == null)
                                {
                                    tmpGroups.Remove(tmpGroup);
                                    continue;
                                }
                            }

                            if (tmpGroups.Count > 0 && tmpGroups.Any(tmpG => !string.IsNullOrEmpty(tmpG.Id) && !_groups.Any(s => s.Id != null && s.Id.Equals(tmpG.Id))))
                            {
                                tmpGroups.Clear();
                                break;
                            }

                            // if no group with matching competitors
                            var groupExists = MergerHelper.FindExistingGroup(groupDTOs, tmpGroup);
                            if(groupExists == null)
                            {
                                tmpGroups.Remove(tmpGroup);
                                continue;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            else
            {
                if (tmpGroups.Count > 0)
                {
                    tmpGroups.Clear();
                }
            }

            foreach (var group in groupDTOs)
            {
                var tempGroup = MergerHelper.FindExistingGroup(tmpGroups, group);
                if (tempGroup == null)
                {
                    tmpGroups.Add(new GroupCI(group, culture));
                }
                else
                {
                    tempGroup.Merge(group, culture);
                }
            }
            _groups = new ReadOnlyCollection<GroupCI>(tmpGroups);
        }

        private void FillCompetitorsReferences(IEnumerable<CompetitorDTO> competitors)
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

        protected override async Task<T> CreateExportableCIAsync<T>()
        {
            var exportable = await base.CreateExportableCIAsync<T>();
            var info = exportable as ExportableTournamentInfoCI;

            info.CategoryId = _categoryId?.ToString();
            info.TournamentCoverage = _tournamentCoverage != null ? await _tournamentCoverage.ExportAsync().ConfigureAwait(false) : null;
            info.Competitors = _competitors?.Select(s=>s.ToString());
            info.CurrentSeasonInfo = _currentSeasonInfo != null ? await _currentSeasonInfo.ExportAsync().ConfigureAwait(false) : null;
            var groupsTasks = _groups?.Select(async g => await g.ExportAsync().ConfigureAwait(false));
            info.Groups = groupsTasks != null ? await Task.WhenAll(groupsTasks) : null;
            info.ScheduleUrns = _scheduleUrns?.Select(s => s.ToString()).ToList();
            info.Round = _round != null ? await _round.ExportAsync().ConfigureAwait(false) : null;
            info.Year = _year;
            info.TournamentInfoBasic = _tournamentInfoBasic != null ? await _tournamentInfoBasic.ExportAsync().ConfigureAwait(false) : null;
            info.ReferenceId = _referenceId?.ReferenceIds?.ToDictionary(r => r.Key, r => r.Value);
            info.SeasonCoverage = _seasonCoverage != null ? await _seasonCoverage.ExportAsync().ConfigureAwait(false) : null;
            info.Seasons = _seasons?.Select(s => s.ToString()).ToList();
            info.LoadedSeasons = new List<CultureInfo>(_loadedSeasons ?? new List<CultureInfo>());
            info.LoadedSchedules = new List<CultureInfo>(_loadedSchedules ?? new List<CultureInfo>());
            info.CompetitorsReferences = _competitorsReferences?.ToDictionary(r => r.Key.ToString(), r => (IDictionary<string, string>)r.Value.ReferenceIds.ToDictionary(v => v.Key, v => v.Value));
            info.ExhibitionGames = _exhibitionGames;

            return exportable;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public override async Task<ExportableCI> ExportAsync() => await CreateExportableCIAsync<ExportableTournamentInfoCI>().ConfigureAwait(false);
    }
}