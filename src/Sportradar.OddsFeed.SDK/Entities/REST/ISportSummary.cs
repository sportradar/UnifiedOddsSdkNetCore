// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    /// Defines a contract implemented by classes representing a sport
    /// </summary>
    public interface ISportSummary : IEntityPrinter
    {
        /// <summary>
        /// Gets a <see cref="Urn"/> uniquely identifying the sport represented by the current instance
        /// </summary>
        Urn Id { get; }

        /// <summary>
        /// Gets the name of the sport represented by the current instance
        /// </summary>
        IReadOnlyDictionary<CultureInfo, string> Names { get; }

        /// <summary>
        /// Gets the name in specified culture language
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns>System.String.</returns>
        string GetName(CultureInfo culture);
    }
}
