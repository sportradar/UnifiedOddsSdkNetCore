/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    ///Implementation of the <see cref="IStageCI"/> interface
    /// </summary>
    /// <seealso cref="CompetitionCI" />
    /// <seealso cref="IStageCI" />
    internal class StageCI : CompetitionCI, IStageCI
    {
        /// <summary>
        /// The category identifier
        /// </summary>
        private URN _categoryId;
        /// <summary>
        /// The parent stage
        /// </summary>
        private URN _parentStageId;
        /// <summary>
        /// The child stages
        /// </summary>
        private IEnumerable<URN> _childStages;

        /// <summary>
        /// The additional parent ids
        /// </summary>
        private IEnumerable<URN> _additionalParentIds;

        private bool _stageScheduleFetched;

        /// <summary>
        /// Initializes a new instance of the <see cref="StageCI"/> class
        /// </summary>
        /// <param name="id">The identifier</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixtureDTO</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixtureDTO timestamps</param>
        public StageCI(URN id,
                       IDataRouterManager dataRouterManager,
                       ISemaphorePool semaphorePool,
                       CultureInfo defaultCulture,
                       MemoryCache fixtureTimestampCache)
            : base(id, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageCI"/> class
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixtureDTO</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixtureDTO timestamps</param>
        public StageCI(StageDTO eventSummary,
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
        /// Initializes a new instance of the <see cref="StageCI"/> class
        /// </summary>
        /// <param name="fixture">The event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixtureDTO</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixtureDTO timestamps</param>
        public StageCI(FixtureDTO fixture,
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
        /// Initializes a new instance of the <see cref="StageCI"/> class
        /// </summary>
        /// <param name="tournamentSummary">The tournament summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixtureDTO</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixtureDTO timestamps</param>
        public StageCI(TournamentInfoDTO tournamentSummary,
                       IDataRouterManager dataRouterManager,
                       ISemaphorePool semaphorePool,
                       CultureInfo currentCulture,
                       CultureInfo defaultCulture,
                       MemoryCache fixtureTimestampCache)
            : base(tournamentSummary, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Merge(tournamentSummary, currentCulture, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageCI"/> class
        /// </summary>
        /// <param name="exportable">The exportable cache item</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixtureDTO</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="MemoryCache"/> used to cache the sport events fixtureDTO timestamps</param>
        public StageCI(ExportableStageCI exportable,
                       IDataRouterManager dataRouterManager,
                       ISemaphorePool semaphorePool,
                       CultureInfo defaultCulture,
                       MemoryCache fixtureTimestampCache)
            : base(exportable, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
            _categoryId = string.IsNullOrEmpty(exportable.CategoryId) ? null : URN.Parse(exportable.CategoryId);
            _parentStageId = string.IsNullOrEmpty(exportable.ParentStageId)
                ? null
                : URN.Parse(exportable.ParentStageId);
            _childStages = exportable.ChildStages?.Select(URN.Parse);
            _additionalParentIds = exportable.AdditionalParentIds.IsNullOrEmpty()
                ? null
                : exportable.AdditionalParentIds.Select(URN.Parse).ToList();
        }

        /// <summary>
        /// get category identifier as an asynchronous operation.
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{URN}" /> representing the asynchronous operation</returns>
        public async Task<URN> GetCategoryIdAsync(IEnumerable<CultureInfo> cultures)
        {
            if (_categoryId != null)
            {
                return _categoryId;
            }
            await FetchMissingSummary(cultures, false).ConfigureAwait(false);
            if (_categoryId == null)
            {
                await FetchMissingFixtures(cultures).ConfigureAwait(false);
            }
            return _categoryId;
        }

        /// <summary>
        /// get parent stage as an asynchronous operation.
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{StageCI}" /> representing the asynchronous operation</returns>
        public async Task<URN> GetParentStageAsync(IEnumerable<CultureInfo> cultures)
        {
            if (_parentStageId != null)
            {
                return _parentStageId;
            }
            await FetchMissingSummary(cultures, false).ConfigureAwait(false);
            return _parentStageId;
        }

        /// <summary>
        /// Asynchronously gets a list of additional ids of the parent stages of the current instance or a null reference if the represented stage does not have the parent stages
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{StageCI}"/> representing the asynchronous operation</returns>
        public async Task<IEnumerable<URN>> GetAdditionalParentStagesAsync(IEnumerable<CultureInfo> cultures)
        {
            if (!_additionalParentIds.IsNullOrEmpty())
            {
                return _additionalParentIds;
            }
            await FetchMissingSummary(cultures, false).ConfigureAwait(false);
            return _additionalParentIds;
        }

        /// <summary>
        /// Get stages as an asynchronous operation.
        /// </summary>
        /// <param name="cultures">The cultures</param>
        /// <returns>A <see cref="Task{T}" /> representing the asynchronous operation</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2486:Generic exceptions should not be ignored", Justification = "Some stages just do not have schedule")]
        public async Task<IEnumerable<URN>> GetStagesAsync(IEnumerable<CultureInfo> cultures)
        {
            if (_childStages != null)
            {
                return _childStages;
            }

            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);

            if (_childStages == null && !_stageScheduleFetched)
            {
                try
                {
                    _stageScheduleFetched = true;
                    var results = await DataRouterManager.GetSportEventsForTournamentAsync(Id, DefaultCulture, this).ConfigureAwait(false);
                    if (results != null)
                    {
                        var sportEventIds = results.ToList();
                        if (!sportEventIds.IsNullOrEmpty())
                        {
                            _childStages = new ReadOnlyCollection<URN>(sportEventIds.Select(r => r.Item1).ToList());
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }

            return _childStages;
        }

        /// <summary>
        /// Merges the specified event summary
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock"></param>
        public void Merge(StageDTO eventSummary, CultureInfo culture, bool useLock)
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
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public void Merge(TournamentInfoDTO eventSummary, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    ActualMergeTournament(eventSummary, culture);
                }
            }
            else
            {
                ActualMergeTournament(eventSummary, culture);
            }
        }

        /// <summary>
        /// Merges the specified event summary
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="culture">The culture</param>
        private void ActualMerge(StageDTO eventSummary, CultureInfo culture)
        {
            base.Merge(eventSummary, culture, false);

            if (eventSummary.Stages != null)
            {
                _childStages = new ReadOnlyCollection<URN>(eventSummary.Stages.Select(r => r.Id).ToList());
            }
            if (eventSummary.Tournament?.Category != null)
            {
                _categoryId = eventSummary.Tournament.Category.Id;
            }
            if (eventSummary.ParentStage != null)
            {
                _parentStageId = eventSummary.ParentStage.Id;
            }
            if (!eventSummary.AdditionalParents.IsNullOrEmpty())
            {
                _additionalParentIds = eventSummary.AdditionalParents.Select(s => s.Id);
            }
        }

        /// <summary>
        /// Merges the specified event summary
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="culture">The culture</param>
        private void ActualMergeTournament(TournamentInfoDTO eventSummary, CultureInfo culture)
        {
            base.Merge(eventSummary, culture, false);
            // could also save tournament live coverage?
            if (eventSummary.Category != null)
            {
                _categoryId = eventSummary.Category.Id;
            }

            if (eventSummary.Competitors != null)
            {
                Competitors = new List<URN>(eventSummary.Competitors.Select(t => t.Id));
            }

            if (eventSummary.Category != null)
            {
                _categoryId = eventSummary.Category.Id;
            }
        }

        /// <summary>
        /// Merges the specified fixtureDTO
        /// </summary>
        /// <param name="fixtureDTO">The fixtureDTO</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public new void MergeFixture(FixtureDTO fixtureDTO, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    base.MergeFixture(fixtureDTO, culture, false);
                    if (fixtureDTO.Tournament?.Category != null)
                    {
                        _categoryId = fixtureDTO.Tournament?.Category?.Id;
                    }
                }
            }
            else
            {
                base.MergeFixture(fixtureDTO, culture, false);
                if (fixtureDTO.Tournament?.Category != null)
                {
                    _categoryId = fixtureDTO.Tournament?.Category?.Id;
                }
            }
        }

        protected override async Task<T> CreateExportableCIAsync<T>()
        {
            var exportable = await base.CreateExportableCIAsync<T>();
            var stage = exportable as ExportableStageCI;

            if (stage == null)
            {
                return null;
            }

            stage.CategoryId = _categoryId?.ToString();
            stage.ParentStageId = _parentStageId?.ToString();
            stage.ChildStages = _childStages?.Select(s => s.ToString());
            stage.AdditionalParentIds = _additionalParentIds?.Select(s => s.ToString()).ToList();

            return exportable;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public override async Task<ExportableCI> ExportAsync() => await CreateExportableCIAsync<ExportableStageCI>().ConfigureAwait(false);
    }
}
