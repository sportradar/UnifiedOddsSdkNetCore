/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a sport event venue
    /// </summary>
    internal class Venue : EntityPrinter, IVenue
    {
        /// <summary>
        /// Gets a <see cref="Urn"/> uniquely identifying the current <see cref="IVenue" /> instance
        /// </summary>
        public Urn Id { get; }

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
        /// Gets a state of the venue represented by current <see cref="IVenue" /> instance
        /// </summary>
        /// <value>The state.</value>
        public string State { get; }

        /// <summary>
        /// Gets the list of courses
        /// </summary>
        /// <value>The list of courses</value>
        public IEnumerable<ICourse> Courses { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Venue"/> class.
        /// </summary>
        /// <param name="ci">A <see cref="VenueCacheItem"/> used to create new instance</param>
        /// <param name="cultures">A culture of the current instance of <see cref="VenueCacheItem"/></param>
        public Venue(VenueCacheItem ci, IReadOnlyCollection<CultureInfo> cultures)
        {
            Guard.Argument(ci, nameof(ci)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            Id = ci.Id;
            Coordinates = ci.Coordinates;
            Capacity = ci.Capacity;

            Names = new ReadOnlyDictionary<CultureInfo, string>(cultures.ToDictionary(c => c, ci.GetName));
            Cities = new ReadOnlyDictionary<CultureInfo, string>(cultures.ToDictionary(c => c, ci.GetCity));
            Countries = new ReadOnlyDictionary<CultureInfo, string>(cultures.ToDictionary(c => c, ci.GetCountry));
            CountryCode = ci.CountryCode;
            State = ci.State;
            Courses = ci.Courses.IsNullOrEmpty() ? new List<Course>() : ci.Courses.Select(s => new Course(s, cultures)).ToList();
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
            return $"Id={Id}, Name[{Names.Keys.First().TwoLetterISOLanguageName}]={Names.Values.First()}";
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

            var course = string.Empty;
            if (!Courses.IsNullOrEmpty())
            {
                course = ", Courses=[" + string.Join(", ", Courses.Select(x => x.ToString("f"))) + "]";
            }

            return $"Id={Id}, Capacity={Capacity}, Coordinates={Coordinates}, Names=[{names}], Cities=[{cityNames}], Countries=[{countryNames}], CountryCode={CountryCode}, State={State}{course}";
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
