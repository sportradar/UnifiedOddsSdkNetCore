/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a sport event venue
    /// </summary>
    internal class Venue : EntityPrinter, IVenue
    {
        /// <summary>
        /// Gets a <see cref="URN"/> uniquely identifying the current <see cref="IVenue" /> instance
        /// </summary>
        public URN Id { get; }

        /// <summary>
        /// Gets the capacity of the venue associated with current <see cref="IVenue" /> instance, or a null
        /// reference if the capacity is not specified
        /// </summary>
        /// <value>The capacity.</value>
        public int? Capacity { get; }

        /// <summary>
        /// Gets a map coordinates specifying the exact location of the venue represented by current <see cref="IVenue" /> instance
        /// </summary>
        public string Coordinates { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}" /> containing venue's names in different languages
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Names { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}" /> containing venue's country names in different languages
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Countries { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}" /> containing venue's city names in different languages
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Cities { get; }

        /// <summary>
        /// Gets a country code of the venue represented by current <see cref="IVenue" /> instance
        /// </summary>
        /// <value>The country code.</value>
        public string CountryCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Venue"/> class.
        /// </summary>
        /// <param name="ci">A <see cref="VenueCI"/> used to create new instance</param>
        /// <param name="cultures">A culture of the current instance of <see cref="VenueCI"/></param>
        public Venue(VenueCI ci, IEnumerable<CultureInfo> cultures)
        {
            Contract.Requires(ci != null);
            Contract.Requires(cultures != null && cultures.Any());

            var cultureList = cultures as IList<CultureInfo> ?? cultures.ToList();

            Id = ci.Id;
            Coordinates = ci.Coordinates;
            Capacity = ci.Capacity;

            Names = new ReadOnlyDictionary<CultureInfo, string>(
                cultureList.Where(c => ci.GetName(c) != null).ToDictionary(c => c, ci.GetName));
            Cities = new ReadOnlyDictionary<CultureInfo, string>(
                cultureList.Where(c => ci.GetCity(c) != null).ToDictionary(c => c, ci.GetCity));
            Countries = new ReadOnlyDictionary<CultureInfo, string>(
                cultureList.Where(c => ci.GetCountry(c) != null).ToDictionary(c => c, ci.GetCountry));
            CountryCode = ci.CountryCode;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing the id of the current instance.</returns>
        protected override string PrintI()
        {
            return $"Id={Id}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance.</returns>
        protected override string PrintC()
        {
            var defaultCulture = new CultureInfo("en");
            var name = Names.ContainsKey(defaultCulture)
                ? Names[defaultCulture]
                : Names.Values.FirstOrDefault();

            return $"Id={Id}, Name={name}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance.</returns>
        protected override string PrintF()
        {
            var names = string.Join(", ", Names.Select(x => x.Key.TwoLetterISOLanguageName + ":" + x.Value));
            var cityNames = string.Join(", ", Cities.Select(x => x.Key.TwoLetterISOLanguageName + ":" + x.Value));
            var countryNames = string.Join(", ", Countries.Select(x => x.Key.TwoLetterISOLanguageName + ":" + x.Value));

            return $"Id={Id}, Capacity={Capacity}, Coordinates={Coordinates}, Names=[{names}], Cities=[{cityNames}], Countries=[{countryNames}], CountryCode={CountryCode}";
        }

        /// <summary>
        /// Prints the JSON representation of the instance
        /// </summary>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }

        /// <summary>
        /// Gets the name for specific locale
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns>Return the name if exists, or null.</returns>
        public string GetName(CultureInfo culture)
        {
            return Names.ContainsKey(culture)
                ? Names[culture]
                : null;
        }

        /// <summary>
        /// Gets the city name for specific locale
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns>Return the city name if exists, or null.</returns>
        public string GetCity(CultureInfo culture)
        {
            return Cities.ContainsKey(culture)
                ? Cities[culture]
                : null;
        }

        /// <summary>
        /// Gets the name for specific locale
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns>Return the Country if exists, or null.</returns>
        public string GetCountry(CultureInfo culture)
        {
            return Countries.ContainsKey(culture)
                ? Countries[culture]
                : null;
        }
    }
}
