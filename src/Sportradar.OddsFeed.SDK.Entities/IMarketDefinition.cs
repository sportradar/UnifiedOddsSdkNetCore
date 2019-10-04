/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.MarketMapping;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines methods used to access market definition properties
    /// </summary>
    public interface IMarketDefinition
    {
        /// <summary>
        /// Returns the unmodified market name template
        /// </summary>
        /// <param name="culture">The culture in which the name template should be provided</param>
        /// <returns>The unmodified market name template</returns>
        string GetNameTemplate(CultureInfo culture);

        /// <summary>
        /// Returns an indication of which kind of outcomes the associated market includes
        /// </summary>
        /// <returns>An indication of which kind of outcomes the associated market includes</returns>
        [Obsolete("Use GetOutcomeType")]
        string GetIncludesOutcomesOfType();

        /// <summary>
        /// Returns an indication of which kind of outcomes the associated market includes
        /// </summary>
        /// <returns>An indication of which kind of outcomes the associated market includes</returns>
        string GetOutcomeType();

        /// <summary>
        /// Returns a list of groups to which the associated market belongs to
        /// </summary>
        /// <returns>a list of groups to which the associated market belongs to</returns>
        IList<string> GetGroups();

        /// <summary>
        /// Returns a dictionary of associated market attributes
        /// </summary>
        /// <returns>A dictionary of associated market attributes</returns>
        IDictionary<string, string> GetAttributes();

        /// <summary>
        /// Returns a list of valid market mappings
        /// </summary>
        /// <returns>a list of valid market mappings</returns>
        IEnumerable<IMarketMappingData> GetValidMappings();
    }
}
