// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Sports
{
    /// <summary>
    /// Represents a cached tournament entity
    /// </summary>
    /// <seealso cref="CacheItem" />
    internal class CategoryCacheItem : CacheItem, IExportableBase
    {
        /// <summary>
        /// Gets the <see cref="Urn"/> specifying the id of the parent sport
        /// </summary>
        public Urn SportId { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{Urn}"/> containing the ids of child tournaments
        /// </summary>
        public IEnumerable<Urn> TournamentIds { get; }

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        public string CountryCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryCacheItem"/> class.
        /// </summary>
        /// <param name="data">A <see cref="CategoryDto"/> containing the category data</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided data</param>
        /// <param name="sportId">The id of the parent sport</param>
        public CategoryCacheItem(CategoryDto data, CultureInfo culture, Urn sportId)
            : base(data.Id, data.Name, culture)
        {
            Guard.Argument(sportId, nameof(sportId)).NotNull();

            TournamentIds = data.Tournaments == null ? null : new ReadOnlyCollection<Urn>(data.Tournaments.Select(i => i.Id).ToList());
            SportId = sportId;
            CountryCode = data.CountryCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryCacheItem"/> class.
        /// </summary>
        /// <param name="data">A <see cref="CategoryDto"/> containing the category data</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided data</param>
        /// <param name="sportId">The id of the parent sport</param>
        /// <param name="tournamentIds">The list of tournament ids</param>
        public CategoryCacheItem(CategorySummaryDto data, CultureInfo culture, Urn sportId, IEnumerable<Urn> tournamentIds)
            : base(data.Id, data.Name, culture)
        {
            Guard.Argument(sportId, nameof(sportId)).NotNull();

            TournamentIds = tournamentIds == null ? null : new ReadOnlyCollection<Urn>(tournamentIds.ToList());
            SportId = sportId;
            CountryCode = data.CountryCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryCacheItem"/> class.
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableCategory" /> representing the cache item</param>
        public CategoryCacheItem(ExportableCategory exportable)
            : base(exportable)
        {
            SportId = Urn.Parse(exportable.SportId);
            TournamentIds = exportable.TournamentIds != null ? new ReadOnlyCollection<Urn>(exportable.TournamentIds.Select(Urn.Parse).ToList()) : null;
            CountryCode = exportable.CountryCode;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportableBase> ExportAsync()
        {
            var exportable = new ExportableCategory
            {
                Id = Id.ToString(),
                Names = new ReadOnlyDictionary<CultureInfo, string>(Name),
                SportId = SportId.ToString(),
                TournamentIds = TournamentIds != null ? new ReadOnlyCollection<string>(TournamentIds.Select(id => id.ToString()).ToList()) : null,
                CountryCode = CountryCode
            };
            return Task.FromResult<ExportableBase>(exportable);
        }
    }
}
