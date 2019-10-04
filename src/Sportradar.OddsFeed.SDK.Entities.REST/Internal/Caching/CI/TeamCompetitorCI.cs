/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Team competitor cache item representation of <see cref="ITeamCompetitor"/>
    /// </summary>
    public class TeamCompetitorCI : CompetitorCI
    {
        /// <summary>
        /// Gets a qualifier additionally describing the competitor (e.g. home, away, ...)
        /// </summary>
        public string Qualifier { get; private set; }

        /// <summary>
        /// Gets the division
        /// </summary>
        /// <value>The division</value>
        public int? Division { get; private set; }

        /// <summary>
        /// Initializes new TeamCompetitorCI instance
        /// </summary>
        /// <param name="competitor">A <see cref="TeamCompetitorDTO"/> to be used to construct new instance</param>
        /// <param name="culture">A culture to be used to construct new instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch missing data</param>
        public TeamCompetitorCI(TeamCompetitorDTO competitor, CultureInfo culture, IDataRouterManager dataRouterManager)
            : base(competitor, culture, dataRouterManager)
        {
            Contract.Requires(competitor != null);
            Contract.Requires(culture != null);

            Merge(competitor, culture);
        }

        /// <summary>
        /// Initializes new TeamCompetitorCI instance
        /// </summary>
        /// <param name="competitor">A <see cref="TeamCompetitorDTO"/> to be used to construct new instance</param>
        public TeamCompetitorCI(CompetitorCI competitor)
            : base(competitor)
        {
            Contract.Requires(competitor != null);
        }

        /// <summary>
        /// Initializes new TeamCompetitorCI instance
        /// </summary>
        /// <param name="competitor">A <see cref="CompetitorDTO"/> to be used to construct new instance</param>
        /// <param name="culture">A culture to be used to construct new instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch missing data</param>
        public TeamCompetitorCI(CompetitorDTO competitor, CultureInfo culture, IDataRouterManager dataRouterManager)
            : base(competitor, culture, dataRouterManager)
        {
            Contract.Requires(competitor != null);
            Contract.Requires(culture != null);

            Merge(competitor, culture);
        }

        /// <summary>
        /// Initializes new TeamCompetitorCI instance
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableTeamCompetitorCI"/> to be used to construct new instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch missing data</param>
        public TeamCompetitorCI(ExportableTeamCompetitorCI exportable, IDataRouterManager dataRouterManager)
            : base(exportable, dataRouterManager)
        {
            Qualifier = exportable.Qualifier;
            Division = exportable.Division;
        }

        /// <summary>
        /// Merges the specified <see cref="TeamCompetitorDTO"/> into instance
        /// </summary>
        /// <param name="competitor">The <see cref="TeamCompetitorDTO"/> used for merge</param>
        /// <param name="culture">The culture of the input <see cref="TeamCompetitorDTO"/></param>
        internal void Merge(TeamCompetitorDTO competitor, CultureInfo culture)
        {
            Contract.Requires(competitor != null);
            Contract.Requires(culture != null);

            base.Merge(competitor, culture);
            Qualifier = competitor.Qualifier;
            Division = competitor.Division;
        }

        /// <summary>
        /// Merges the specified <see cref="TeamCompetitorCI"/> into instance
        /// </summary>
        /// <param name="item">The <see cref="TeamCompetitorCI"/> used for merge</param>
        internal void Merge(TeamCompetitorCI item)
        {
            base.Merge(item);

            Qualifier = item.Qualifier ?? Qualifier;
            Division = item.Division ?? Division;
        }

        protected override async Task<T> CreateExportableCIAsync<T>()
        {
            var exportable = await base.CreateExportableCIAsync<T>().ConfigureAwait(false);
            var team = exportable as ExportableTeamCompetitorCI;
            team.Qualifier = Qualifier;
            team.Division = Division;

            return team as T;
        }


        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public override async Task<ExportableCI> ExportAsync() => await CreateExportableCIAsync<ExportableTeamCompetitorCI>().ConfigureAwait(false);
    }
}