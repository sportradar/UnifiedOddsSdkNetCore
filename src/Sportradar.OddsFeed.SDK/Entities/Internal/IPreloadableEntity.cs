// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// Represents an entity that supports asynchronous preloading from the API source.
    /// </summary>
    internal interface IPreloadableEntity
    {
        /// <summary>
        /// Asynchronously retrieves and populates the entity's data from the API.
        /// </summary>
        /// <param name="languages">Languages to fetch the summary for.</param>
        /// <param name="requestOptions">Options for the request.</param>
        /// <returns>A task that represents the asynchronous preload operation.</returns>
        Task EnsureSummaryIsFetchedForLanguages(IReadOnlyCollection<CultureInfo> languages, RequestOptions requestOptions);
    }
}
