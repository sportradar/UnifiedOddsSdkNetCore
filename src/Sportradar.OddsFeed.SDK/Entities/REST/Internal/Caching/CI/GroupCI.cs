/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
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
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name of the group
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{URN}"/> representing group competitors
        /// </summary>
        public IEnumerable<URN> CompetitorsIds { get; private set; }

        /// <summary>
        /// The competitors references
        /// </summary>
        public IDictionary<URN, ReferenceIdCI> CompetitorsReferences { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupCI"/> class
        /// </summary>
        /// <param name="group">A <see cref="GroupDTO"/> containing group information</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided group information</param>
        internal GroupCI(GroupDTO group, CultureInfo culture)
        {
            Guard.Argument(group, nameof(group)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            Merge(group, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupCI"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableGroupCI"/> containing group information</param>
        internal GroupCI(ExportableGroupCI exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            Id = exportable.Id;
            Name = exportable.Name;
            CompetitorsIds = exportable.Competitors?.Select(URN.Parse).ToList();
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
                var tempCompetitors = CompetitorsIds == null ? new List<URN>() : new List<URN>(CompetitorsIds);
                var tempCompetitorsReferences = CompetitorsReferences == null ? new Dictionary<URN, ReferenceIdCI>() : new Dictionary<URN, ReferenceIdCI>(CompetitorsReferences);

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
                        tempCompetitorsReferences.Add(competitor.Id, new ReferenceIdCI(competitor.ReferenceIds));
                    }
                }

                CompetitorsIds = new ReadOnlyCollection<URN>(tempCompetitors);
                CompetitorsReferences = new ReadOnlyDictionary<URN, ReferenceIdCI>(tempCompetitorsReferences);
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
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public Task<ExportableGroupCI> ExportAsync()
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
                        SdkLoggerFactory.GetLoggerForExecution(typeof(GroupCI)).LogError(e, "Exporting GroupCI");
                    }
                }
            }

            return Task.FromResult(new ExportableGroupCI
            {
                Id = Id,
                Name = Name,
                Competitors = CompetitorsIds?.Select(s => s.ToString()),
                CompetitorsReferences = cr.IsNullOrEmpty() ? null : cr
            });
        }
    }
}
