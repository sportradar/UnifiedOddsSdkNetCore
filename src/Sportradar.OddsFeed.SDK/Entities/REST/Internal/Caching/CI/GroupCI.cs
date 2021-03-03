/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dawn;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// A implementation of cache item for Group
    /// </summary>
    internal class GroupCI
    {
        /// <summary>
        /// Gets the id of the group
        /// </summary>
        public string Id { get; private set;}

        /// <summary>
        /// Gets the name of the group
        /// </summary>
        public string Name { get; private set;}

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{ICompetitorCI}"/> representing group competitors
        /// </summary>
        public IEnumerable<CompetitorCI> Competitors { get; private set; }

        /// <summary>
        /// The competitors references
        /// </summary>
        public IDictionary<URN, ReferenceIdCI> CompetitorsReferences { get; private set; }

        private readonly IDataRouterManager _dataRouterManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupCI"/> class
        /// </summary>
        /// <param name="group">A <see cref="GroupDTO"/> containing group information</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided group information</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch missing data</param>
        internal GroupCI(GroupDTO group, CultureInfo culture, IDataRouterManager dataRouterManager)
        {
            Guard.Argument(group, nameof(group)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            _dataRouterManager = dataRouterManager;
            Merge(group, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupCI"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableGroupCI"/> containing group information</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch missing data</param>
        internal GroupCI(ExportableGroupCI exportable, IDataRouterManager dataRouterManager)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            _dataRouterManager = dataRouterManager;
            Id = exportable.Id;
            Name = exportable.Name;
            Competitors = exportable.Competitors?.Select(c => new CompetitorCI(c, dataRouterManager)).ToList();
            CompetitorsReferences = exportable.CompetitorsReferences?.ToDictionary(c => URN.Parse(c.Key), c => new ReferenceIdCI(c.Value));
        }

        /// <summary>
        /// Merges the provided group information with the information held by the current instance
        /// </summary>
        /// <param name="group">A <see cref="GroupDTO"/> containing group information</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided group information</param>
        internal void Merge(GroupDTO group, CultureInfo culture)
        {
            Guard.Argument(group, nameof(group)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();
            
            if(string.IsNullOrEmpty(Id) || !string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(group.Id))
            {
                Id = group.Id;
            }
            if(string.IsNullOrEmpty(Name) || !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(group.Name))
            {
                Name = group.Name;
            }

            if (!group.Competitors.IsNullOrEmpty())
            {
                var tempCompetitors = Competitors == null ? new List<CompetitorCI>() : new List<CompetitorCI>(Competitors);
                
                if (tempCompetitors.Count > 0 && tempCompetitors.Count != group.Competitors.Count())
                {
                    tempCompetitors.Clear();
                }

                if (!group.Competitors.All(a => tempCompetitors.Select(s => s.Id).Contains(a.Id)))
                {
                    tempCompetitors.Clear();
                }

                foreach (var competitor in group.Competitors)
                {
                    var tempCompetitor = tempCompetitors.FirstOrDefault(c => c.Id.Equals(competitor.Id));
                    if (tempCompetitor == null)
                    {
                        tempCompetitors.Add(new CompetitorCI(competitor, culture, _dataRouterManager));
                    }
                    else
                    {
                        tempCompetitor.Merge(competitor, culture);
                    }
                }

                Competitors = new ReadOnlyCollection<CompetitorCI>(tempCompetitors);
                CompetitorsReferences = new ReadOnlyDictionary<URN, ReferenceIdCI>(tempCompetitors.ToDictionary(c => c.Id, c => c.ReferenceId));
            }
            else
            {
                Competitors = null;
                CompetitorsReferences = null;
            }
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public async Task<ExportableGroupCI> ExportAsync()
        {
            var competitorsTask = Competitors?.Select(async c => await c.ExportAsync().ConfigureAwait(false) as ExportableCompetitorCI);

            return new ExportableGroupCI
            {
                Id = Id,
                Competitors = competitorsTask != null ? await Task.WhenAll(competitorsTask) : null,
                CompetitorsReferences = CompetitorsReferences?.ToDictionary(c => c.Key.ToString(), c => c.Value.ReferenceIds.ToDictionary(r => r.Key, r => r.Value)),
                Name = Name
            };
        }
    }
}
