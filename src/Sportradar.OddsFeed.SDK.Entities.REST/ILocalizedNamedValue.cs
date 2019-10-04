/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Specifies a contract implemented by classes representing values with localized / translatable descriptions
    /// </summary>
    /// <seealso cref="INamedValue" />
    public interface ILocalizedNamedValue : INamedValue
    {
        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing translated descriptions
        /// </summary>
        IReadOnlyDictionary<CultureInfo, string> Descriptions { get; }

        /// <summary>
        /// Gets the description for specific locale
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns>Return the Description if exists, or null.</returns>
        string GetDescription(CultureInfo culture);
    }
}