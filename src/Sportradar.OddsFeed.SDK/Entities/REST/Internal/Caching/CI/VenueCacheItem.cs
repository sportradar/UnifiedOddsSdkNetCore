/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Provides information about venue of a sport event
    /// </summary>
    internal class VenueCacheItem : SportEntityCacheItem
    {
        /// <summary>
        /// A <see cref="IDictionary{CultureInfo, String}"/> containing venue name in different languages
        /// </summary>
        private readonly IDictionary<CultureInfo, string> _names;

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo, String}"/> containing city of the venue in different languages
        /// </summary>
        private readonly IDictionary<CultureInfo, string> _cityNames;

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo, String}"/> containing country of the venue in different languages
        /// </summary>
        private readonly IDictionary<CultureInfo, string> _countryNames;

        /// <summary>
        /// Gets the capacity of the venue associated with current <see cref="VenueCacheItem" /> instance, or a null
        /// reference if the capacity is not specified
        /// </summary>
        /// <value>The capacity.</value>
        public int? Capacity { get; private set; }

        /// <summary>
        /// Gets a map coordinates specifying the exact location of the venue represented by current <see cref="VenueCacheItem" /> instance
        /// </summary>
        public string Coordinates { get; private set; }

        /// <summary>
        /// Gets a country code of the venue represented by current <see cref="VenueCacheItem" /> instance
        /// </summary>
        public string CountryCode { get; private set; }

        /// <summary>
        /// Gets a state of the venue represented by current <see cref="VenueCacheItem" /> instance
        /// </summary>
        public string State { get; private set; }

        /// <summary>
        /// Gets the course
        /// </summary>
        /// <value>The course</value>
        public ICollection<CourseCacheItem> Courses { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VenueCacheItem"/> class
        /// </summary>
        /// <param name="venue">A <see cref="VenueDto"/> containing information about a venue</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the <c>dto</c></param>
        internal VenueCacheItem(VenueDto venue, CultureInfo culture)
            : base(venue)
        {
            Guard.Argument(venue, nameof(venue)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            _names = new Dictionary<CultureInfo, string>();
            _countryNames = new Dictionary<CultureInfo, string>();
            _cityNames = new Dictionary<CultureInfo, string>();
            Courses = new List<CourseCacheItem>();

            Merge(venue, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VenueCacheItem"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableVenue"/> containing information about a venue</param>
        public VenueCacheItem(ExportableVenue exportable)
            : base(exportable)
        {
            _names = exportable.Names.IsNullOrEmpty() ? new Dictionary<CultureInfo, string>() : new Dictionary<CultureInfo, string>(exportable.Names);
            _cityNames = exportable.CityNames.IsNullOrEmpty() ? new Dictionary<CultureInfo, string>() : new Dictionary<CultureInfo, string>(exportable.CityNames);
            _countryNames = exportable.CountryNames.IsNullOrEmpty() ? new Dictionary<CultureInfo, string>() : new Dictionary<CultureInfo, string>(exportable.CountryNames);
            Capacity = exportable.Capacity;
            Coordinates = exportable.Coordinates;
            CountryCode = exportable.CountryCode;
            State = exportable.State;
            Courses = exportable.Courses.IsNullOrEmpty()
                ? new List<CourseCacheItem>()
                : exportable.Courses.Select(s => new CourseCacheItem(s)).ToList();
        }

        /// <summary>
        /// Merges information from the provided <see cref="VenueDto"/> into the current instance
        /// </summary>
        /// <param name="venue">A <see cref="VenueDto"/> containing information about a venue</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the input <see cref="VenueDto"/></param>
        internal void Merge(VenueDto venue, CultureInfo culture)
        {
            Guard.Argument(venue, nameof(venue)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            Capacity = venue.Capacity;
            Coordinates = venue.Coordinates;
            _names[culture] = venue.Name;
            _countryNames[culture] = venue.Country;
            _cityNames[culture] = venue.City;
            CountryCode = venue.CountryCode;
            State = venue.State;
            if (!venue.Courses.IsNullOrEmpty())
            {
                MergeCourses(venue.Courses.ToList(), culture);
            }
        }

        private void MergeCourses(ICollection<CourseDto> courseDtos, CultureInfo culture)
        {
            if (Courses.IsNullOrEmpty())
            {
                Courses = courseDtos.Select(s => new CourseCacheItem(s, culture)).ToList();
                return;
            }

            var tempCourses = new List<CourseCacheItem>();

            foreach (var courseDto in courseDtos)
            {
                var course = Courses.FirstOrDefault(c => Equals(c.Id, courseDto.Id));
                if (course == null)
                {
                    tempCourses.Add(new CourseCacheItem(courseDto, culture));
                }
                else
                {
                    course.Merge(courseDto, culture);
                    tempCourses.Add(course);
                }
            }

            Courses = tempCourses;
        }

        /// <summary>
        /// Gets the name of the venue in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned name</param>
        /// <returns>The name of the venue in the specified language if it exists. Null otherwise.</returns>
        public string GetName(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            return _names.TryGetValue(culture, out var name)
                ? name
                : null;
        }

        /// <summary>
        /// Gets the city name of the venue in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned city name</param>
        /// <returns>The city name of the venue in the specified language if it exists. Null otherwise.</returns>
        public string GetCity(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            return _cityNames.TryGetValue(culture, out var cityName)
                ? cityName
                : null;
        }

        /// <summary>
        /// Gets the country name of the venue in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned country name</param>
        /// <returns>The country name of the venue in the specified language if it exists. Null otherwise.</returns>
        public string GetCountry(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            return _countryNames.TryGetValue(culture, out var countryName)
                ? countryName
                : null;
        }

        /// <summary>
        /// Determines whether the current instance has translations for the specified languages
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the required languages</param>
        /// <returns>True if the current instance contains data in the required locals. Otherwise false.</returns>
        public bool HasTranslationsFor(IEnumerable<CultureInfo> cultures)
        {
            return cultures.All(c => _names.ContainsKey(c));
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableVenue"/> instance containing all relevant properties</returns>
        public Task<ExportableVenue> ExportAsync()
        {
            return Task.FromResult(new ExportableVenue
            {
                Id = Id.ToString(),
                Names = new ReadOnlyDictionary<CultureInfo, string>(_names),
                CityNames = new ReadOnlyDictionary<CultureInfo, string>(_cityNames),
                CountryNames = new ReadOnlyDictionary<CultureInfo, string>(_countryNames),
                Capacity = Capacity,
                Coordinates = Coordinates,
                CountryCode = CountryCode,
                State = State,
                Courses = Courses?.Select(s => new ExportableCourse { Id = s.Id.ToString(), Names = s.Names, Holes = s.Holes.Select(h => new ExportableHole { Number = h.Number, Par = h.Par }).ToList() }).ToList()
            });
        }
    }
}
