// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    /// Defines a contract for classes implementing basic entity information, containing Id as <see cref="Urn"/> and translatable Name
    /// </summary>
    public interface IBaseEntity : IEntityPrinter
    {
        /// <summary>
        /// Gets the <see cref="Urn"/> identifying the current instance
        /// </summary>
        /// <value>The <see cref="Urn"/> identifying the current instance</value>
        Urn Id { get; }

        /// <summary>
        /// Gets the name associated with this instance in specific language
        /// </summary>
        /// <param name="culture">The language used to get the name</param>
        /// <returns>Name if available in specified language or null</returns>
        string GetName(CultureInfo culture);

        /// <summary>
        /// Gets the list of translated names
        /// </summary>
        /// <value>The list of translated names</value>
        IReadOnlyDictionary<CultureInfo, string> Names { get; }
    }
}
