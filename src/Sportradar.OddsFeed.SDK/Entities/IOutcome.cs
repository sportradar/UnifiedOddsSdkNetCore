// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.Rest.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Represent a betting market outcome
    /// </summary>
    public interface IOutcome
    {
        /// <summary>
        /// Gets the value uniquely identifying the current instance
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Asynchronously gets the name of the outcome in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language in which to get the name</param>
        /// <returns>A <see cref="Task{String}"/> representing the async operation</returns>
        Task<string> GetNameAsync(CultureInfo culture);

        /// <summary>
        /// Asynchronously gets the mapping Ids of the specified outcome
        /// </summary>
        /// <returns>Returns the mapping Ids of the specified outcome</returns>
        Task<IEnumerable<IOutcomeMapping>> GetMappedOutcomeIdsAsync();

        /// <summary>
        /// Gets the associated outcome definition instance
        /// </summary>
        IOutcomeDefinition OutcomeDefinition { get; }
    }
}
