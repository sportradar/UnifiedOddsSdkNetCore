/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing delayed info in a sport event
    /// </summary>
    public interface IDelayedInfo
    {
        /// <summary>
        /// Gets the id identifying the current instance
        /// </summary>
        /// <value>The id identifying the current instance</value>
        int Id { get; }
        /// <summary>
        /// 
        /// Gets the list of translated names
        /// </summary>
        /// <value>The list of translated names</value>
        IReadOnlyDictionary<CultureInfo, string> Descriptions { get; }

        /// <summary>
        /// Gets the description associated with this instance in specific language
        /// </summary>
        /// <param name="culture">The language used to get the description</param>
        /// <returns>Description if available in specified language or null</returns>
        string GetDescription(CultureInfo culture);
    }
}