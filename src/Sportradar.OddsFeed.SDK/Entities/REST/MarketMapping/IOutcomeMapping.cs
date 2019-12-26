/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping
{
    /// <summary>
    /// Defines a contract implemented by classes representing mapping id of outcome
    /// </summary>
    public interface IOutcomeMapping
    {
        /// <summary>
        /// Gets the identifier
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the name of the outcome in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language in which to get the name</param>
        /// <returns>The name in the specific language</returns>
        string GetName(CultureInfo culture);

        /// <summary>
        /// Gets the id of the mapped market
        /// </summary>
        /// <value>The id of the mapped market</value>
        string MarketId { get; }
    }
}
