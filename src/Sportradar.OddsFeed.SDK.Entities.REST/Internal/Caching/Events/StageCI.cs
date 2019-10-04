/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    ///Implementation of the <see cref="IStageCI"/> interface
    /// </summary>
    /// <seealso cref="CompetitionCI" />
    /// <seealso cref="IStageCI" />
    public class StageCI : CompetitionCI, IStageCI
    {
        /// <summary>
        /// The category identifier
        /// </summary>
        private URN _categoryId;
        /// <summary>
        /// The parent stage
        /// </summary>
        private StageCI _parentStage;
        /// <summary>
        /// The child stages
        /// </summary>
        private IEnumerable<StageCI> _childStages;
        /// <summary>
        /// The stage type
        /// </summary>
        private StageType _stageType;

        /// <summary>
        /// Initializes a new instance of the <see cref="StageCI"/> class
        /// </summary>
        /// <param name="id">The identifier</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public StageCI(URN id,
                       IDataRouterManager dataRouterManager,
                       ISemaphorePool semaphorePool,
                       CultureInfo defaultCulture,
                       ObjectCache fixtureTimestampCache)
            : base(id, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageCI"/> class
        /// </summary>
        /// <param name="eventSummary">The event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public StageCI(StageDTO eventSummary,
                       IDataRouterManager dataRouterManager,
                       ISemaphorePool semaphorePool,
                       CultureInfo currentCulture,
                       CultureInfo defaultCulture,
                       ObjectCache fixtureTimestampCache)
            : base(eventSummary, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Merge(eventSummary, currentCulture, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageCI"/> class
        /// </summary>
        /// <param name="fixture">The event summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public StageCI(FixtureDTO fixture,
                        IDataRouterManager dataRouterManager,
                        ISemaphorePool semaphorePool,
                        CultureInfo currentCulture,
                        CultureInfo defaultCulture,
                        ObjectCache fixtureTimestampCache)
            : base(fixture, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Merge(fixture, currentCulture, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageCI"/> class
        /// </summary>
        /// <param name="tournamentSummary">The tournament summary</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="currentCulture">The current culture</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public StageCI(TournamentInfoDTO tournamentSummary,
                        IDataRouterManager dataRouterManager,
                        ISemaphorePool semaphorePool,
                        CultureInfo currentCulture,
                        CultureInfo defaultCulture,
                        ObjectCache fixtureTimestampCache)
            : base(tournamentSummary, dataRouterManager, semaphorePool, currentCulture, defaultCulture, fixtureTimestampCache)
        {
            Merge(tournamentSummary, currentCulture, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageCI"/> class
        /// </summary>
        /// <param name="exportable">The exportable cache item</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">The semaphore pool</param>
        /// <param name="defaultCulture">The default culture</param>
        /// <param name="fixtureTimestampCache">A <see cref="ObjectCache"/> used to cache the sport events fixture timestamps</param>
        public StageCI(ExportableStageCI exportable,
            IDataRouterManager dataRouterManager,
            ISemaphorePool semaphorePool,
            CultureInfo defaultCulture,
            ObjectCache fixtureTimestampCache)
            : base(exportable, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache)
        {
            _categoryId = URN.Parse(exportable.CategoryId);
            _parentStage = exportable.ParentStage != null ? new StageCI(exportable.ParentStage, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache) : null;
            _childStages = exportable.ChildStages?.Select(s => new StageCI(s, dataRouterManager, semaphorePool, defaultCulture, fixtureTimestampCache));
            _stageType = exportable.StageType;
        }

        /// <summary>
        /// get category identifier as an asynchronous operation.
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
        /// get parent stage as an asynchronous operation.
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the returned instance should be translated</param>
        /// <returns>A <see cref="Task{StageCI}" /> representing the asynchronous operation</returns>
        public async Task<StageCI> GetParentStageAsync(IEnumerable<CultureInfo> cultures)
        {
            if (_parentStage != null)
            {
                return _parentStage;
            }
            ////the requested data does not contain translatable values - fetch just for default language
            await FetchMissingSummary(cultures, false).ConfigureAwait(false);
            return _parentStage;
        }

        /// <summary>
        /// get stages as an asynchronous operation.
        /// </summary>
        /// <param name="cultures">The cultures</param>
        /// <returns>A <see cref="Task{T}" /> representing the asynchronous operation</returns>
        public async Task<IEnumerable<StageCI>> GetStagesAsync(IEnumerable<CultureInfo> cultures)
        {
            if (_childStages != null)
            {
                return _childStages;
            }
            ////the requested data does not contain translatable values - fetch just for default language
            await FetchMissingSummary(cultures, false).ConfigureAwait(false);
            return _childStages;
        }

        /// <summary>
        /// get type as an asynchronous operation.
        /// </summary>
        /// <returns>A <see cref="Task{URN}" /> representing the asynchronous operation</returns>
        public async Task<StageType> GetTypeAsync()
        {
            await FetchMissingSummary(new[] { DefaultCulture }, false).ConfigureAwait(false);
            return _stageType;
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
        private void ActualMerge(StageDTO eventSummary, CultureInfo culture)
        {
            base.Merge(eventSummary, culture, false);

            if (eventSummary.ParentStage != null)
            {
                // no translatable data - just replace with new value
                _parentStage = new StageCI(eventSummary.ParentStage, DataRouterManager, SemaphorePool, culture, DefaultCulture, FixtureTimestampCache);
            }
            else
            {
                if (eventSummary.Tournament != null)
                {
                    _parentStage = new StageCI(eventSummary.Tournament.Id, DataRouterManager, SemaphorePool, culture, FixtureTimestampCache);
                }
            }
            if (eventSummary.Stages != null)
            {
                // no translatable data - just replace with new value
                _childStages = new ReadOnlyCollection<StageCI>(eventSummary
                                                              .Stages.Select(r => new StageCI(r, DataRouterManager, SemaphorePool, culture,
                                                                                              DefaultCulture, FixtureTimestampCache)).ToList());
            }

            if (eventSummary.Type != null)
            {
                _stageType = eventSummary.Type.Value == SportEventType.Parent
                                 ? StageType.Parent
                                 : StageType.Child;
            }

            if (eventSummary.Tournament?.Category != null)
            {
                _categoryId = eventSummary.Tournament.Category.Id;
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
        private void ActualMergeTournament(TournamentInfoDTO eventSummary, CultureInfo culture)
        {
            base.Merge(eventSummary, culture, false);

            if (eventSummary.Category != null)
            {
                _categoryId = eventSummary.Category.Id;
            }

            if (eventSummary.Type != null)
            {
                _stageType = eventSummary.Type.Value == SportEventType.Parent
                                 ? StageType.Parent
                                 : StageType.Child;
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
        /// Merges the specified fixture
        /// </summary>
        /// <param name="fixture">The fixture</param>
        /// <param name="culture">The culture</param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        public new void MergeFixture(FixtureDTO fixture, CultureInfo culture, bool useLock)
        {
            if (useLock)
            {
                lock (MergeLock)
                {
                    base.MergeFixture(fixture, culture, false);
                    _categoryId = fixture.Tournament?.Category?.Id;

                    if (fixture.Type != null)
                    {
                        _stageType = fixture.Type.Value == SportEventType.Parent
                                         ? StageType.Parent
                                         : StageType.Child;
                    }
                }
            }
            else
            {
                base.MergeFixture(fixture, culture, false);
                _categoryId = fixture.Tournament?.Category?.Id;

                if (fixture.Type != null)
                {
                    _stageType = fixture.Type.Value == SportEventType.Parent
                                     ? StageType.Parent
                                     : StageType.Child;
                }
            }
        }

        protected override async Task<T> CreateExportableCIAsync<T>()
        {
            var exportable = await base.CreateExportableCIAsync<T>();
            var stage = exportable as ExportableStageCI;

            stage.CategoryId = _categoryId?.ToString();
            stage.ParentStage = _parentStage != null ? await _parentStage.ExportAsync().ConfigureAwait(false) as ExportableStageCI : null;
            var childTasks = _childStages?.Select(async s => await s.ExportAsync().ConfigureAwait(false) as ExportableStageCI);
            stage.ChildStages = childTasks != null ? await Task.WhenAll(childTasks) : null;
            stage.StageType = _stageType;

            return exportable;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public override async Task<ExportableCI> ExportAsync() => await CreateExportableCIAsync<ExportableStageCI>().ConfigureAwait(false);
    }
}
