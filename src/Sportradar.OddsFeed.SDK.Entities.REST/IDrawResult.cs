/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing the draw result
    /// </summary>
    public interface IDrawResult
    {
        /// <summary>
        /// Gets the value of the draw
        /// </summary>
        int? Value { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing translated names
        /// </summary>
        IReadOnlyDictionary<CultureInfo, string> Names { get; }

        /// <summary>
        /// Gets the name in specified culture language
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>System.String.</returns>
        string GetName(CultureInfo culture);
    }
}
