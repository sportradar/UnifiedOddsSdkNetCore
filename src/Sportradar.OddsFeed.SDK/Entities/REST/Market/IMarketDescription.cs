// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.Rest.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Market
{
    /// <summary>
    /// Defines a contract implemented by classes representing market description
    /// </summary>
    public interface IMarketDescription
    {
        /// <summary>
        /// Gets the id of the market described by the current instance
        /// </summary>
        long Id { get; }

        /// <summary>
        /// Gets the <see cref="IEnumerable{IOutcomeDescription}"/> describing the outcomes of the market
        /// described by the current instance
        /// </summary>
        IEnumerable<IOutcomeDescription> Outcomes { get; }

        /// <summary>
        /// Gets the <see cref="IEnumerable{ISpecifier}"/> representing the specifiers of the market
        /// described by the current instance
        /// </summary>
        IEnumerable<ISpecifier> Specifiers { get; }

        /// <summary>
        /// Gets the <see cref="IEnumerable{IMarketMapping}"/> representing the mappings of the market
        /// described by the current instance
        /// </summary>
        IEnumerable<IMarketMappingData> Mappings { get; }

        /// <summary>
        /// Gets the <see cref="IEnumerable{IMarketAttribute}"/> representing market attributes providing
        /// additional information about the market.
        /// </summary>
        IEnumerable<IMarketAttribute> Attributes { get; }

        /// <summary>
        /// Gets the outcome_type market attribute - an indication of which type of outcomes the market includes
        /// </summary>
        string OutcomeType { get; }

        /// <summary>
        /// Gets a list of groups to which the market belongs to
        /// </summary>
        IEnumerable<string> Groups { get; }

        /// <summary>
        /// Gets the name of the market description in the language specified by the passed <c>culture</c>
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the retrieved name</param>
        /// <returns>Returns the name in specific language</returns>
        string GetName(CultureInfo culture);

        /// <summary>
        /// Gets the description of the market description in the language specified by the passed <c>culture</c>
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the retrieved description</param>
        /// <returns>Returns the description of the market description in the language specified by the passed <c>culture</c></returns>
        string GetDescription(CultureInfo culture);
    }
}
