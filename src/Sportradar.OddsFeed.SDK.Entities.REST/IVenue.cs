/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a sport event venue
    /// </summary>
    public interface IVenue : IEntityPrinter
    {
        /// <summary>
        /// Gets a <see cref="URN"/> uniquely identifying the current <see cref="IVenue"/> instance
        /// </summary>
        URN Id { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing venue's names in different languages
        /// </summary>
        IReadOnlyDictionary<CultureInfo, string> Names { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing venue's city names in different languages
        /// </summary>
        IReadOnlyDictionary<CultureInfo, string> Cities { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing venue's country names in different languages
        /// </summary>
        IReadOnlyDictionary<CultureInfo, string> Countries { get; }

        /// <summary>
        /// Gets the capacity of the venue associated with current <see cref="IVenue"/> instance, or a null
        /// reference if the capacity is not specified
        /// </summary>
        int? Capacity { get; }

        /// <summary>
        /// Gets a map coordinates specifying the exact location of the venue represented by current <see cref="IVenue"/> instance
        /// </summary>
        string Coordinates { get; }

        /// <summary>
        /// Gets the name for specific locale
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>Return the name if exists, or null</returns>
        string GetName(CultureInfo culture);

        /// <summary>
        /// Gets the city name for specific locale
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>Return the city name if exists, or null</returns>
        string GetCity(CultureInfo culture);

        /// <summary>
        /// Gets the country name for specific locale
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>Return the country name if exists, or null</returns>
        string GetCountry(CultureInfo culture);

        /// <summary>
        /// Gets a country code of the venue represented by current <see cref="IVenue" /> instance
        /// </summary>
        string CountryCode { get; }
    }
}
