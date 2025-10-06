// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// A implementation of cache item for Group
    /// </summary>
    internal class GroupCacheItem
    {
        /// <summary>
        /// Gets the id of the group
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name of the group
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{Urn}"/> representing group competitors
        /// </summary>
        public IEnumerable<Urn> CompetitorsIds { get; private set; }

        /// <summary>
        /// The competitors references
        /// </summary>
        public IDictionary<Urn, ReferenceIdCacheItem> CompetitorsReferences { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupCacheItem"/> class
        /// </summary>
        /// <param name="group">A <see cref="GroupDto"/> containing group information</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided group information</param>
        internal GroupCacheItem(GroupDto group, CultureInfo culture)
        {
            Guard.Argument(group, nameof(group)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            Merge(group, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupCacheItem"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableGroup"/> containing group information</param>
        internal GroupCacheItem(ExportableGroup exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            Id = exportable.Id;
            Name = exportable.Name;
            CompetitorsIds = exportable.Competitors?.Select(Urn.Parse).ToList();
            CompetitorsReferences = exportable.CompetitorsReferences?.ToDictionary(c => Urn.Parse(c.Key), c => new ReferenceIdCacheItem(c.Value));
        }

        /// <summary>
        /// Merges the provided group information with the information held by the current instance
        /// </summary>
        /// <param name="group">A <see cref="GroupDto"/> containing group information</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided group information</param>
        internal void Merge(GroupDto group, CultureInfo culture)
        {
            Guard.Argument(group, nameof(group)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            if (string.IsNullOrEmpty(Id) || (!string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(group.Id)))
            {
                Id = group.Id;
            }
            if (string.IsNullOrEmpty(Name) || (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(group.Name)))
            {
                Name = group.Name;
            }

            if (!group.Competitors.IsNullOrEmpty())
            {
                var tempCompetitors = CompetitorsIds == null ? new List<Urn>() : new List<Urn>(CompetitorsIds);
                var tempCompetitorsReferences = CompetitorsReferences == null ? new Dictionary<Urn, ReferenceIdCacheItem>() : new Dictionary<Urn, ReferenceIdCacheItem>(CompetitorsReferences);

                if (tempCompetitors.Count > 0 && tempCompetitors.Count != group.Competitors.Count())
                {
                    tempCompetitors.Clear();
                }

                if (!group.Competitors.All(a => tempCompetitors.Contains(a.Id)))
                {
                    tempCompetitors.Clear();
                }

                foreach (var competitor in group.Competitors)
                {
                    var tempCompetitor = tempCompetitors.FirstOrDefault(c => c.Equals(competitor.Id));
                    if (tempCompetitor == null)
                    {
                        tempCompetitors.Add(competitor.Id);
                        tempCompetitorsReferences.Add(competitor.Id, new ReferenceIdCacheItem(competitor.ReferenceIds));
                    }
                }

                CompetitorsIds = new ReadOnlyCollection<Urn>(tempCompetitors);
                CompetitorsReferences = new ReadOnlyDictionary<Urn, ReferenceIdCacheItem>(tempCompetitorsReferences);
            }
            else
            {
                CompetitorsIds = null;
                CompetitorsReferences = null;
            }
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportableGroup> ExportAsync()
        {
            var cr = new Dictionary<string, Dictionary<string, string>>();
            if (!CompetitorsReferences.IsNullOrEmpty())
            {
                foreach (var competitorsReference in CompetitorsReferences)
                {
                    try
                    {
                        var refs = competitorsReference.Value.ReferenceIds?.ToDictionary(r => r.Key, r => r.Value);
                        cr.Add(competitorsReference.Key.ToString(), refs);
                    }
                    catch (Exception e)
                    {
                        SdkLoggerFactory.GetLoggerForExecution(typeof(GroupCacheItem)).LogError(e, "Exporting GroupCacheItem");
                    }
                }
            }

            return Task.FromResult(new ExportableGroup
            {
                Id = Id,
                Name = Name,
                Competitors = CompetitorsIds?.Select(s => s.ToString()),
                CompetitorsReferences = cr.IsNullOrEmpty() ? null : cr
            });
        }
    }
}
