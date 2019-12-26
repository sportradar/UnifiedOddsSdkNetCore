/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dawn;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Sports
{
    /// <summary>
    /// Represents a cached tournament entity
    /// </summary>
    /// <seealso cref="CacheItem" />
    internal class CategoryCI : CacheItem, IExportableCI
    {
        /// <summary>
        /// Gets the <see cref="URN"/> specifying the id of the parent sport
        /// </summary>
        public URN SportId { get; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{URN}"/> containing the ids of child tournaments
        /// </summary>
        public IEnumerable<URN> TournamentIds { get; }

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        public string CountryCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryCI"/> class.
        /// </summary>
        /// <param name="data">A <see cref="CategoryDTO"/> containing the category data</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided data</param>
        /// <param name="sportId">The id of the parent sport</param>
        public CategoryCI(CategoryDTO data, CultureInfo culture, URN sportId)
            : base(data.Id, data.Name, culture)
        {
            Guard.Argument(sportId, nameof(sportId)).NotNull();

            TournamentIds = data.Tournaments == null ? null : new ReadOnlyCollection<URN>(data.Tournaments.Select(i => i.Id).ToList());
            SportId = sportId;
            CountryCode = data.CountryCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryCI"/> class.
        /// </summary>
        /// <param name="data">A <see cref="CategoryDTO"/> containing the category data</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided data</param>
        /// <param name="sportId">The id of the parent sport</param>
        /// <param name="tournamentIds">The list of tournament ids</param>
        public CategoryCI(CategorySummaryDTO data, CultureInfo culture, URN sportId, IEnumerable<URN> tournamentIds)
            : base(data.Id, data.Name, culture)
        {
            Guard.Argument(sportId, nameof(sportId)).NotNull();

            TournamentIds = tournamentIds == null ? null : new ReadOnlyCollection<URN>(tournamentIds.ToList());
            SportId = sportId;
            CountryCode = data.CountryCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryCI"/> class.
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableCategoryCI" /> representing the cache item</param>
        public CategoryCI(ExportableCategoryCI exportable)
            : base(exportable)
        {
            SportId = URN.Parse(exportable.SportId);
            TournamentIds = exportable.TournamentIds != null ? new ReadOnlyCollection<URN>(exportable.TournamentIds.Select(URN.Parse).ToList()) : null;
            CountryCode = exportable.CountryCode;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public Task<ExportableCI> ExportAsync()
        {
            var exportable = new ExportableCategoryCI
            {
                Id = Id.ToString(),
                Name = new ReadOnlyDictionary<CultureInfo, string>(Name),
                SportId = SportId.ToString(),
                TournamentIds = TournamentIds != null ? new ReadOnlyCollection<string>(TournamentIds.Select(id => id.ToString()).ToList()) : null,
                CountryCode = CountryCode
            };
            return Task.FromResult<ExportableCI>(exportable);
        }
    }
}
