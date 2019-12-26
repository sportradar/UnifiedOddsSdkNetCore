/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Market
{
    /// <summary>
    /// Defines a contract implemented by classes representing betting outcome description
    /// </summary>
    public interface IOutcomeDescription {

        /// <summary>
        /// Gets a value uniquely identifying the current outcome within the market
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the name of the betting outcome represented by the current instance
        /// </summary>
        string GetName(CultureInfo culture);

        /// <summary>
        /// Gets the description of the betting outcome represented by the current instance
        /// </summary>
        string GetDescription(CultureInfo culture);
    }
}