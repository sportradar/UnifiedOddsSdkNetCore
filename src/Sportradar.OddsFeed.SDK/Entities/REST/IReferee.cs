/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing sport event referee
    /// </summary>
    public interface IReferee : IEntityPrinter
    {
        /// <summary>
        /// Gets a <see cref="URN"/> used to uniquely identify the current <see cref="IReferee"/> instance
        /// </summary>
        URN Id { get; }

        /// <summary>
        /// Gets the name of the referee represented by the current <see cref="IReferee"/> instance
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing referee nationality in different languages
        /// </summary>
        IReadOnlyDictionary<CultureInfo, string> Nationalities { get; }

        /// <summary>
        /// Gets the referee nationality in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language.</param>
        /// <returns>The referee nationality in the specified language.</returns>
        string GetNationality(CultureInfo culture);
    }
}