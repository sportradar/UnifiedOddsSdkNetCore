using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public class TestCompetitor : ICompetitor
    {
        public TestCompetitor(URN id, string name, CultureInfo culture)
        {
            Id = id;
            Names = new Dictionary<CultureInfo, string> { { culture, name } };
        }

        public TestCompetitor(URN id, IDictionary<CultureInfo, string> names)
        {
            Id = id;
            Names = new ReadOnlyDictionary<CultureInfo, string>(names);
        }

        /// <summary>
        /// Returns a string that represents the current object
        /// </summary>
        /// <param name="formatProvider">A format provider used to format the output string</param>
        /// <returns>A string that represents the current object.</returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return Id.ToString();
        }

        /// <summary>Formats the value of the current instance using the specified format.</summary>
        /// <returns>The value of the current instance in the specified format.</returns>
        /// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
        /// <param name="formatProvider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
        public string ToString(string format, IFormatProvider formatProvider = null)
        {
            return Id.ToString();
        }

        /// <summary>
        /// Gets the <see cref="URN"/> uniquely identifying the current <see cref="ICompetitor" /> instance
        /// </summary>
        public URN Id { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing player names in different languages
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Names { get; }

        /// <summary>
        /// Gets the name of the player in the specified language or a null reference
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned name</param>
        /// <returns>The name of the player in the specified language or a null reference.</returns>
        public string GetName(CultureInfo culture)
        {
            return Names[culture];
        }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing competitor's country names in different languages
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Countries { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing competitor's abbreviations in different languages
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Abbreviations { get; }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="ICompetitor"/> is virtual - i.e.
        /// competes in a virtual sport
        /// </summary>
        public bool IsVirtual { get; }

        /// <summary>
        /// Gets the reference ids
        /// </summary>
        public IReference References { get; }

        /// <summary>
        /// Gets the competitor's country name in the specified language or a null reference.
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the country name.</param>
        /// <returns>The competitor's country name in the specified language or a null reference.</returns>
        public string GetCountry(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the competitor's abbreviation in the specified language or a null reference.
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the abbreviation.</param>
        /// <returns>The competitor's abbreviation in the specified language or a null reference.</returns>
        public string GetAbbreviation(CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        public string CountryCode { get; }

        /// <summary>
        /// Gets the list of associated player ids
        /// </summary>
        /// <value>The associated player ids</value>
        public IEnumerable<IPlayer> AssociatedPlayers { get; }

        /// <summary>
        /// Gets the jerseys of known competitors
        /// </summary>
        /// <value>The jerseys</value>
        public IEnumerable<IJersey> Jerseys { get; }

        /// <summary>
        /// Gets the manager
        /// </summary>
        /// <value>The manager</value>
        public IManager Manager { get; }

        /// <summary>
        /// Gets the venue
        /// </summary>
        /// <value>The venue</value>
        public IVenue Venue { get; }

        /// <summary>
        /// Gets the gender
        /// </summary>
        /// <value>The gender</value>
        public string Gender { get; }

        /// <summary>
        /// Gets the race driver profile
        /// </summary>
        /// <value>The race driver profile</value>
        public IRaceDriverProfile RaceDriverProfile { get; }

        /// <summary>
        /// Gets the age group
        /// </summary>
        /// <value>The age group</value>
        public string AgeGroup { get; }

        /// <summary>
        /// Gets the state
        /// </summary>
        /// <value>The state</value>
        public string State { get; }
    }
}
