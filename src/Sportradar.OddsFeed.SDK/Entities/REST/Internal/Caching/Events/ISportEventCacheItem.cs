// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent cached information about a sport event
    /// </summary>
    internal interface ISportEventCacheItem
    {
        /// <summary>
        /// Fetches sport event detail info for those of the specified languages which are not yet fetched
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the required languages</param>
        /// <param name="requestOptions">Request options</param>
        /// <param name="forceFetch">Should the cached data be ignored</param>
        /// <returns>A <see cref="Task" /> representing the async operation</returns>
        Task FetchMissingSummary(IEnumerable<CultureInfo> cultures, RequestOptions requestOptions, bool forceFetch);

        /// <summary>
        /// Gets a <see cref="Urn"/> specifying the id of the sport event associated with the current instance
        /// </summary>
        Urn Id { get; }

        /// <summary>
        /// Asynchronously gets the names of the sport event for specific cultures
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the required languages</param>
        /// <returns>Return a name of the race, or match</returns>
        Task<IReadOnlyDictionary<CultureInfo, string>> GetNamesAsync(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Asynchronously gets a <see cref="Urn"/> representing sport id
        /// </summary>
        /// <returns>The <see cref="Urn"/> of the sport this sport event belongs to</returns>
        Task<Urn> GetSportIdAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="DateTime"/> instance specifying when the sport event associated with the current
        /// instance was scheduled, or a null reference if the time is not known.
        /// </summary>
        /// <returns>A <see cref="DateTime"/> instance specifying when the sport event associated with the current
        /// instance was scheduled, or a null reference if the time is not known.</returns>
        Task<DateTime?> GetScheduledAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="DateTime" /> instance specifying for when the sport event associated with the current instance is
        /// scheduled to end or a null reference if the value is not known
        /// </summary>
        /// <returns>A <see cref="Task{DateTime}"/> representing the retrieval operation</returns>
        Task<DateTime?> GetScheduledEndAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="bool"/> specifying if the start time to be determined is set for the associated sport event.
        /// </summary>
        /// <returns>A <see cref="bool"/> specifying if the start time to be determined is set for the associated sport event.</returns>
        Task<bool?> GetStartTimeTbdAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="Urn"/> specifying the replacement sport event for the associated sport event.
        /// </summary>
        /// <returns>A <see cref="Urn"/> specifying the replacement sport event for the associated sport event.</returns>
        Task<Urn> GetReplacedByAsync();

        /// <summary>
        /// Determines whether the current instance has translations for the specified languages
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the required languages</param>
        /// <returns>True if the current instance contains data in the required locals. Otherwise false.</returns>
        bool HasTranslationsFor(IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Merges the specified <see cref="SportEventSummaryDto"/> into instance
        /// </summary>
        /// <param name="dto">The <see cref="SportEventSummaryDto"/> used for merge</param>
        /// <param name="culture">The culture of the input <see cref="SportEventSummaryDto"/></param>
        /// <param name="useLock">Should the lock mechanism be used during merge</param>
        void Merge(SportEventSummaryDto dto, CultureInfo culture, bool useLock);
    }
}
