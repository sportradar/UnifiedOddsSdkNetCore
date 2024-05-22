// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// An implementation of the Manager cache item
    /// </summary>
    internal class ManagerCacheItem : CacheItem
    {
        /// <summary>
        /// Gets a <see cref="IDictionary{CultureInfo,String}"/> containing translated nationality of the item
        /// </summary>
        public IDictionary<CultureInfo, string> Nationality { get; }

        /// <summary>
        /// Gets the country code of the manager
        /// </summary>
        /// <value>The country code of the manager</value>
        public string CountryCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerCacheItem"/> class
        /// </summary>
        /// <param name="item">The dto with manager info</param>
        /// <param name="culture">The culture</param>
        public ManagerCacheItem(ManagerDto item, CultureInfo culture)
            : base(item.Id, item.Name, culture)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            Nationality = new Dictionary<CultureInfo, string>();
            Nationality[culture] = item.Nationality;
            CountryCode = item.CountryCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerCacheItem"/> class
        /// </summary>
        /// <param name="exportable">The <see cref="ExportableManager"/> with manager info</param>
        public ManagerCacheItem(ExportableManager exportable)
            : base(Urn.Parse(exportable.Id), new Dictionary<CultureInfo, string>(exportable.Names))
        {
            Nationality = exportable.Nationality != null
                ? new Dictionary<CultureInfo, string>(exportable.Nationality)
                : null;
            CountryCode = exportable.CountryCode;
        }

        /// <summary>
        /// Merges the specified item
        /// </summary>
        /// <param name="item">The item with the manager info</param>
        /// <param name="culture">The culture.</param>
        public void Merge(ManagerDto item, CultureInfo culture)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            base.Merge(item, culture);

            Nationality[culture] = item.Nationality;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableManager"/> instance containing all relevant properties</returns>
        public Task<ExportableManager> ExportAsync()
        {
            return Task.FromResult(new ExportableManager
            {
                Id = Id.ToString(),
                Names = new ReadOnlyDictionary<CultureInfo, string>(Name),
                Nationality = Nationality != null ? new ReadOnlyDictionary<CultureInfo, string>(Nationality) : null,
                CountryCode = CountryCode
            });
        }
    }
}
