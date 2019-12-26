/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes implementing basic entity information, containing Id as <see cref="URN"/> and translatable Name
    /// </summary>
    public interface IBaseEntity : IEntityPrinter
    {
        /// <summary>
        /// Gets the <see cref="URN"/> identifying the current instance
        /// </summary>
        /// <value>The <see cref="URN"/> identifying the current instance</value>
        URN Id { get; }

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
