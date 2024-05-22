// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Sports
{
    /// <summary>
    /// Represents a cached sport entity
    /// </summary>
    /// <seealso cref="CacheItem" />
    internal class SportCacheItem : CacheItem, IExportableBase
    {
        /// <summary>
        /// Gets <see cref="ICollection{Urn}"/> specifying the id's of child categories
        /// </summary>
        /// 
        public ICollection<Urn> CategoryIds
        {
            get;
            private set;
        }

        /// <summary>
        /// The loaded categories for tournament
        /// </summary>
        private readonly List<CultureInfo> _loadedCategories = new List<CultureInfo>();

        /// <summary>
        /// Lock object used for loading categories
        /// </summary>
        private readonly SemaphoreSlim _loadedCategoriesSemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// The <see cref="IDataRouterManager"/> used to obtain categories
        /// </summary>
        private readonly IDataRouterManager _dataRouterManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportCacheItem"/> class.
        /// </summary>
        /// <param name="data">A <see cref="SportDto" /> instance containing sport data</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain categories</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided data</param>
        public SportCacheItem(SportDto data, IDataRouterManager dataRouterManager, CultureInfo culture)
            : base(data.Id, data.Name, culture)
        {
            CategoryIds = new Collection<Urn>();
            if (data.Categories != null)
            {
                CategoryIds = new Collection<Urn>(data.Categories.Select(i => i.Id).ToList());
            }

            _dataRouterManager = dataRouterManager;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportCacheItem"/> class.
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableSport" /> representing the cache item</param>
        public SportCacheItem(ExportableSport exportable)
            : base(exportable)
        {
            CategoryIds = exportable.CategoryIds != null ? new Collection<Urn>(exportable.CategoryIds.Select(Urn.Parse).ToList()) : new Collection<Urn>();
            _loadedCategories = exportable.LoadedCategories != null ? new List<CultureInfo>(exportable.LoadedCategories) : new List<CultureInfo>();
        }

        /// <summary>
        /// Merges the information from the provided <see cref="CacheItem" /> to data held by current instance
        /// </summary>
        /// <param name="item">A <see cref="CacheItem" /> containing the data to be merged to current instance</param>
        /// <param name="culture">A <see cref="CultureInfo" /> specifying the culture of data in the passed <see cref="CacheItem" /></param>
        public override void Merge(CacheItem item, CultureInfo culture)
        {
            base.Merge(item, culture);
            var sportCacheItem = item as SportCacheItem;
            if (sportCacheItem?.CategoryIds != null && sportCacheItem.CategoryIds.Any())
            {
                CategoryIds = new Collection<Urn>(sportCacheItem.CategoryIds.Concat(CategoryIds ?? new List<Urn>()).Distinct().ToList());
            }

            if (sportCacheItem?._loadedCategories?.Any() ?? false)
            {
                _loadedCategories.AddRange(LanguageHelper.GetMissingCultures(sportCacheItem._loadedCategories, _loadedCategories));
            }
        }

        public async Task LoadCategoriesAsync(IEnumerable<CultureInfo> cultures)
        {
            var wantedCultures = cultures as ICollection<CultureInfo> ?? cultures.ToList();
            try
            {
                await _loadedCategoriesSemaphore.WaitAsync().ConfigureAwait(false);
                wantedCultures = LanguageHelper.GetMissingCultures(wantedCultures, _loadedCategories);
                if (!wantedCultures.Any())
                {
                    return;
                }

                foreach (var culture in wantedCultures)
                {
                    await _dataRouterManager.GetSportCategoriesAsync(Id, culture).ConfigureAwait(false);
                }
                _loadedCategories.AddRange(wantedCultures);
            }
            finally
            {
                _loadedCategoriesSemaphore.ReleaseSafe();
            }
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportableBase> ExportAsync()
        {
            var exportable = new ExportableSport
            {
                Id = Id.ToString(),
                Names = new ReadOnlyDictionary<CultureInfo, string>(Name),
                CategoryIds = CategoryIds != null ? new ReadOnlyCollection<string>(CategoryIds.Select(id => id.ToString()).ToList()) : null,
                LoadedCategories = new ReadOnlyCollection<CultureInfo>(_loadedCategories)
            };
            return Task.FromResult<ExportableBase>(exportable);
        }
    }
}
