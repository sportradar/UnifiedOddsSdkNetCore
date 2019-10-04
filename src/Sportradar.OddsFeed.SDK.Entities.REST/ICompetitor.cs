/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Represents a team competing in a sport event
    /// </summary>
    public interface ICompetitor : IPlayer
    {
        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing competitor's country names in different languages
        /// </summary>
        [DataMember]
        IReadOnlyDictionary<CultureInfo, string> Countries { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing competitor's abbreviations in different languages
        /// </summary>
        [DataMember]
        IReadOnlyDictionary<CultureInfo, string> Abbreviations { get; }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="ICompetitor"/> is virtual - i.e.
        /// competes in a virtual sport
        /// </summary>
        [DataMember]
        bool IsVirtual { get; }

        /// <summary>
        /// Gets the reference ids
        /// </summary>
        [DataMember]
        IReference References { get; }

        /// <summary>
        /// Gets the competitor's country name in the specified language or a null reference.
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the country name.</param>
        /// <returns>The competitor's country name in the specified language or a null reference.</returns>
        string GetCountry(CultureInfo culture);

        /// <summary>
        /// Gets the competitor's abbreviation in the specified language or a null reference.
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the abbreviation.</param>
        /// <returns>The competitor's abbreviation in the specified language or a null reference.</returns>
        string GetAbbreviation(CultureInfo culture);

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        string CountryCode { get; }

        /// <summary>
        /// Gets the list of associated player ids
        /// </summary>
        /// <value>The associated player ids</value>
        IEnumerable<IPlayer> AssociatedPlayers { get; }

        /// <summary>
        /// Gets the jerseys of known competitors
        /// </summary>
        /// <value>The jerseys</value>
        IEnumerable<IJersey> Jerseys { get; }

        /// <summary>
        /// Gets the manager
        /// </summary>
        /// <value>The manager</value>
        IManager Manager { get; }

        /// <summary>
        /// Gets the venue
        /// </summary>
        /// <value>The venue</value>
        IVenue Venue { get; }
    }
}
