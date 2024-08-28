// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Globalization;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Team competitor cache item representation of <see cref="ITeamCompetitor"/>
    /// </summary>
    internal class TeamCompetitorCacheItem : CompetitorCacheItem
    {
        /// <summary>
        /// Gets a qualifier additionally describing the competitor (e.g. home, away, ...)
        /// </summary>
        public string Qualifier { get; private set; }

        /// <summary>
        /// Initializes new TeamCompetitorCacheItem instance
        /// </summary>
        /// <param name="competitor">A <see cref="TeamCompetitorDto"/> to be used to construct new instance</param>
        /// <param name="culture">A culture to be used to construct new instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch missing data</param>
        public TeamCompetitorCacheItem(TeamCompetitorDto competitor, CultureInfo culture, IDataRouterManager dataRouterManager)
            : base(competitor, culture, dataRouterManager)
        {
            Guard.Argument(competitor, nameof(competitor)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            Merge(competitor, culture);
        }

        /// <summary>
        /// Initializes new TeamCompetitorCacheItem instance
        /// </summary>
        /// <param name="competitor">A <see cref="TeamCompetitorDto"/> to be used to construct new instance</param>
        public TeamCompetitorCacheItem(CompetitorCacheItem competitor)
            : base(competitor)
        {
            Guard.Argument(competitor, nameof(competitor)).NotNull();
        }

        /// <summary>
        /// Initializes new TeamCompetitorCacheItem instance
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableTeamCompetitor"/> to be used to construct new instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch missing data</param>
        public TeamCompetitorCacheItem(ExportableTeamCompetitor exportable, IDataRouterManager dataRouterManager)
            : base(exportable, dataRouterManager)
        {
            Qualifier = exportable.Qualifier;
        }

        internal void Import(ExportableTeamCompetitor exportable)
        {
            base.Import(exportable);

            Qualifier = exportable.Qualifier;
        }

        /// <summary>
        /// Merges the specified <see cref="TeamCompetitorDto"/> into instance
        /// </summary>
        /// <param name="competitor">The <see cref="TeamCompetitorDto"/> used for merge</param>
        /// <param name="culture">The culture of the input <see cref="TeamCompetitorDto"/></param>
        internal void Merge(TeamCompetitorDto competitor, CultureInfo culture)
        {
            Guard.Argument(competitor, nameof(competitor)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            base.Merge(competitor, culture);
            Qualifier = competitor.Qualifier;
        }

        /// <summary>
        /// Merges the specified <see cref="TeamCompetitorCacheItem"/> into instance
        /// </summary>
        /// <param name="item">The <see cref="TeamCompetitorCacheItem"/> used for merge</param>
        internal void Merge(TeamCompetitorCacheItem item)
        {
            base.Merge(item);

            Qualifier = item.Qualifier ?? Qualifier;
        }

        protected override async Task<T> CreateExportableBaseAsync<T>()
        {
            var exportable = await base.CreateExportableBaseAsync<T>().ConfigureAwait(false);
            var team = exportable as ExportableTeamCompetitor;
            if (team != null)
            {
                team.Qualifier = Qualifier;
            }

            return team as T;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public override async Task<ExportableBase> ExportAsync()
        {
            return await CreateExportableBaseAsync<ExportableTeamCompetitor>().ConfigureAwait(false);
        }
    }
}
