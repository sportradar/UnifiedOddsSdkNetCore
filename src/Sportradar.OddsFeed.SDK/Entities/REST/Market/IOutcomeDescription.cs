// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Market
{
    /// <summary>
    /// Defines a contract implemented by classes representing betting outcome description
    /// </summary>
    public interface IOutcomeDescription
    {
        /// <summary>
        /// Gets a value uniquely identifying the current outcome within the market
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the name of the betting outcome represented by the current instance
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the retrieved name</param>
        /// <returns>Returns the name in specific language</returns>
        string GetName(CultureInfo culture);

        /// <summary>
        /// Gets the description of the betting outcome represented by the current instance
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the retrieved description</param>
        /// <returns>Returns the description of the outcome description in the language specified by the passed <c>culture</c></returns>
        string GetDescription(CultureInfo culture);
    }
}
