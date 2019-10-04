/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing sport category
    /// </summary>
    public interface ICategorySummary : IEntityPrinter
    {
        /// <summary>
        /// Gets a <see cref="URN"/> uniquely identifying the category represented by the current instance
        /// </summary>
        URN Id { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing translated category name
        /// </summary>
        IReadOnlyDictionary<CultureInfo, string> Names { get; }

        /// <summary>
        /// Gets the name in specified culture language
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns>System.String.</returns>
        string GetName(CultureInfo culture);

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        string CountryCode { get; }
    }
}
