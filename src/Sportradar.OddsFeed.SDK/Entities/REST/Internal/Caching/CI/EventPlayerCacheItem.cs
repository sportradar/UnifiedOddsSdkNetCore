// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// A cache item for event player
    /// Implements the <see cref="CacheItem" />
    /// </summary>
    /// <seealso cref="CacheItem" />
    internal class EventPlayerCacheItem : CacheItem
    {
        /// <summary>
        /// Gets the bench value
        /// </summary>
        /// <value>The bench value - in case of yellow or red card event, it is relevant to know if the player who is getting the card is sitting on the bench at that exact moment.</value>
        /// <remarks>The attribute is equal to 1 if the player who gets the card is sitting on the bench. In case the player who gets the card is on the field, then the attribute is not added at all.</remarks>
        public string Bench { get; }

        /// <summary>
        /// Gets the method value
        /// </summary>
        /// <value>The method value</value>
        /// <remarks>The attribute can assume values such as 'penalty' and 'own goal'. In case the attribute is not inserted, then the goal is not own goal neither penalty.</remarks>
        public string Method { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventPlayerCacheItem"/> class.
        /// </summary>
        /// <param name="dto">A <see cref="EventPlayerDto"/> data</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the provided data</param>
        public EventPlayerCacheItem(EventPlayerDto dto, CultureInfo culture)
            : base(dto.Id, dto.Name, culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            Bench = dto.Bench;
            Method = dto.Method;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventPlayerCacheItem"/> class.
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableBase"/> representing the cache item</param>
        public EventPlayerCacheItem(ExportableEventPlayer exportable) : base(exportable)
        {
            Bench = exportable.Bench;
            Method = exportable.Method;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventPlayerCacheItem"/> class.
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> representing the id of the item</param>
        /// <param name="name">The name of the item</param>
        protected EventPlayerCacheItem(Urn id, IDictionary<CultureInfo, string> name) : base(id, name)
        {
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportableEventPlayer> ExportAsync()
        {
            return Task.FromResult(new ExportableEventPlayer
            {
                Id = Id.ToString(),
                Names = new Dictionary<CultureInfo, string>(Name ?? new Dictionary<CultureInfo, string>()),
                Bench = Bench,
                Method = Method
            });
        }
    }
}
