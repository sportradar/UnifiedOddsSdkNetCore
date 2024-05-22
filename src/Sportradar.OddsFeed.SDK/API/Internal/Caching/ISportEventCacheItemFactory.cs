// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// Defines a contract implemented by classes used to built <see cref="SportEventCacheItem"/> instance
    /// </summary>
    internal interface ISportEventCacheItemFactory
    {
        /// <summary>
        /// Builds a <see cref="SportEventCacheItem"/> instance from the provided sport event id
        /// </summary>
        /// <param name="eventId">A <see cref="Urn"/> representing the id of the sport event</param>
        /// <returns>a new instance of <see cref="SportEventCacheItem"/> instance</returns>
        SportEventCacheItem Build(Urn eventId);

        /// <summary>
        /// Builds a <see cref="SportEventCacheItem"/> instance from the provided <see cref="SportEventSummaryDto"/> instance
        /// </summary>
        /// <param name="eventSummary">A <see cref="SportEventSummaryDto"/> instance containing basic sport event info</param>
        /// <param name="currentCulture">A <see cref="CultureInfo"/> of the input <see cref="SportEventSummaryDto"/> data</param>
        /// <returns>A new instance of <see cref="SportEventCacheItem"/> instance</returns>
        SportEventCacheItem Build(SportEventSummaryDto eventSummary, CultureInfo currentCulture);

        /// <summary>
        /// Builds a <see cref="SportEventCacheItem"/> instance from the provided <see cref="FixtureDto"/> instance
        /// </summary>
        /// <param name="fixture">A <see cref="FixtureDto"/> instance containing fixture (pre-match) related sport event info</param>
        /// <param name="currentCulture">A <see cref="CultureInfo"/> of the input <see cref="FixtureDto"/> data</param>
        /// <returns>A new instance of <see cref="SportEventCacheItem"/> instance</returns>
        SportEventCacheItem Build(FixtureDto fixture, CultureInfo currentCulture);

        /// <summary>
        /// Builds a <see cref="SportEventCacheItem"/> instance from the provided exportable cache item
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableBase"/> representing the sport event</param>
        /// <returns>a new instance of <see cref="SportEventCacheItem"/> instance</returns>
        SportEventCacheItem Build(ExportableBase exportable);

        /// <summary>
        /// Gets a derived <see cref="SportEventCacheItem"/> instance from the cache object
        /// </summary>
        /// <param name="cacheItem">A <see cref="SportEventCacheItem"/> instance from the cache</param>
        /// <returns>A new instance of <see cref="SportEventCacheItem"/> instance</returns>
        SportEventCacheItem Get(object cacheItem);

        /// <summary>
        /// Gets a <see cref="ICacheStore{T}"/> used to cache fixture timestamps
        /// </summary>
        /// <returns>A <see cref="ICacheStore{T}"/> used to cache fixture timestamps</returns>
        ICacheStore<string> GetFixtureTimestampCache();
    }
}
