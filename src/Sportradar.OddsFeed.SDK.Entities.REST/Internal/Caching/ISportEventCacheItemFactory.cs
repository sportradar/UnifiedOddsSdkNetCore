/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Globalization;
using System.Runtime.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    /// <summary>
    /// Defines a contract implemented by classes used to built <see cref="SportEventCI"/> instance
    /// </summary>
    internal interface ISportEventCacheItemFactory
    {
        /// <summary>
        /// Builds a <see cref="SportEventCI"/> instance from the provided sport event id
        /// </summary>
        /// <param name="id">A <see cref="URN"/> representing the id of the sport event</param>
        /// <returns>a new instance of <see cref="SportEventCI"/> instance</returns>
        SportEventCI Build(URN id);

        /// <summary>
        /// Builds a <see cref="SportEventCI"/> instance from the provided <see cref="SportEventSummaryDTO"/> instance
        /// </summary>
        /// <param name="eventSummary">A <see cref="SportEventSummaryDTO"/> instance containing basic sport event info</param>
        /// <param name="currentCulture">A <see cref="CultureInfo"/> of the input <see cref="SportEventSummaryDTO"/> data</param>
        /// <returns>A new instance of <see cref="SportEventCI"/> instance</returns>
        SportEventCI Build(SportEventSummaryDTO eventSummary, CultureInfo currentCulture);

        /// <summary>
        /// Builds a <see cref="SportEventCI"/> instance from the provided <see cref="FixtureDTO"/> instance
        /// </summary>
        /// <param name="fixture">A <see cref="FixtureDTO"/> instance containing fixture (pre-match) related sport event info</param>
        /// <param name="currentCulture">A <see cref="CultureInfo"/> of the input <see cref="FixtureDTO"/> data</param>
        /// <returns>A new instance of <see cref="SportEventCI"/> instance</returns>
        SportEventCI Build(FixtureDTO fixture, CultureInfo currentCulture);

        /// <summary>
        /// Builds a <see cref="SportEventCI"/> instance from the provided exportable cache item
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableCI"/> representing the the sport event</param>
        /// <returns>a new instance of <see cref="SportEventCI"/> instance</returns>
        SportEventCI Build(ExportableCI exportable);

        /// <summary>
        /// Gets a derived <see cref="SportEventCI"/> instance from the cache object
        /// </summary>
        /// <param name="cacheItem">A <see cref="SportEventCI"/> instance from the cache</param>
        /// <returns>A new instance of <see cref="SportEventCI"/> instance</returns>
        SportEventCI Get(object cacheItem);

        /// <summary>
        /// Gets a <see cref="ObjectCache"/> used to cache fixture timestamps
        /// </summary>
        /// <returns>A <see cref="ObjectCache"/> used to cache fixture timestamps</returns>
        ObjectCache GetFixtureTimestampCache();
    }
}
