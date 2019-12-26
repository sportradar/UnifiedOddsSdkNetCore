/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing
    /// </summary>
    public interface ISeasonInfo
    {
        /// <summary>
        /// Gets the <see cref="URN"/> identifying the current instance
        /// </summary>
        /// <value>The <see cref="URN"/> identifying the current instance</value>
        URN Id { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>System.String</returns>
        string GetName(CultureInfo culture);

        /// <summary>
        /// Gets the list of translated names
        /// </summary>
        /// <value>The list of translated names</value>
        IReadOnlyDictionary<CultureInfo, string> Names { get; }
    }
}
