// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Defines a cache item for fixture delayed info
    /// </summary>
    internal class DelayedInfoCacheItem
    {
        /// <summary>
        /// Gets the identifier
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo,String}"/> containing descriptions in different languages
        /// </summary>
        public readonly IDictionary<CultureInfo, string> Descriptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayedInfoCacheItem"/> class
        /// </summary>
        /// <param name="dto">The <see cref="DelayedInfoDto"/> used to create new instance</param>
        /// <param name="culture">The culture of the input <see cref="DelayedInfoDto"/></param>
        internal DelayedInfoCacheItem(DelayedInfoDto dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            Descriptions = new Dictionary<CultureInfo, string>();
            Merge(dto, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayedInfoCacheItem"/> class
        /// </summary>
        /// <param name="exportable">The <see cref="ExportableDelayedInfo"/> used to create new instance</param>
        internal DelayedInfoCacheItem(ExportableDelayedInfo exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            Id = exportable.Id;
            Descriptions = new Dictionary<CultureInfo, string>(exportable.Descriptions ?? new Dictionary<CultureInfo, string>());
        }

        /// <summary>
        /// Merges the specified <see cref="DelayedInfoCacheItem"/> into instance
        /// </summary>
        /// <param name="dto">The <see cref="DelayedInfoDto"/> used for merging</param>
        /// <param name="culture">The culture of the input <see cref="DelayedInfoDto"/></param>
        internal void Merge(DelayedInfoDto dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();
            Id = dto.Id;
            Descriptions[culture] = dto.Description;
        }

        /// <summary>
        /// Gets the name for specific locale
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>Return the Name if exists, or null</returns>
        public string GetDescription(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            return Descriptions == null || !Descriptions.ContainsKey(culture)
                ? null
                : Descriptions[culture];
        }

        /// <summary>
        /// Determines whether the current instance has translations for the specified languages
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the required languages</param>
        /// <returns>True if the current instance contains data in the required locals. Otherwise false</returns>
        public virtual bool HasTranslationsFor(IEnumerable<CultureInfo> cultures)
        {
            return cultures.All(c => Descriptions.ContainsKey(c));
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportableDelayedInfo> ExportAsync()
        {
            return Task.FromResult(new ExportableDelayedInfo
            {
                Id = Id,
                Descriptions = new Dictionary<CultureInfo, string>(Descriptions ?? new Dictionary<CultureInfo, string>())
            });
        }
    }
}
