/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// A cache item for event player assist
    /// </summary>
    /// <seealso cref="CacheItem" />
    internal class EventPlayerAssistCacheItem : CacheItem
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public string Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventPlayerAssistCacheItem"/> class.
        /// </summary>
        /// <param name="dto">The dto containing data</param>
        /// <param name="culture">The culture of the data</param>
        public EventPlayerAssistCacheItem(EventPlayerAssistDto dto, CultureInfo culture)
            : base(dto.Id, dto.Name, culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            Type = dto.Type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventPlayerAssistCacheItem"/> class.
        /// </summary>
        /// <param name="exportable">The <see cref="ExportableEventPlayerAssist"/></param>
        public EventPlayerAssistCacheItem(ExportableEventPlayerAssist exportable)
            : base(exportable)
        {
            Type = exportable.Type;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportableEventPlayerAssist> ExportAsync()
        {
            return Task.FromResult(new ExportableEventPlayerAssist
            {
                Id = Id.ToString(),
                Names = new Dictionary<CultureInfo, string>(Name ?? new Dictionary<CultureInfo, string>()),
                Type = Type
            });
        }
    }
}
