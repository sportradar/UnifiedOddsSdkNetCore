// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-access-object representing a venue
    /// </summary>
    /// <seealso cref="SportEntityDto" />
    internal class VenueDto : SportEntityDto
    {
        /// <summary>
        /// Gets the capacity of the represented venue or a null reference if value is not known
        /// </summary>
        internal int? Capacity { get; }

        /// <summary>
        /// Gets the city of the represented venue
        /// </summary>
        internal string City { get; }

        /// <summary>
        /// Gets the country of the represented venue
        /// </summary>
        internal string Country { get; }

        /// <summary>
        /// Gets the country code of the represented venue
        /// </summary>
        internal string CountryCode { get; }

        /// <summary>
        /// Gets the state of the represented venue
        /// </summary>
        internal string State { get; }

        /// <summary>
        /// Gets the GPS coordinates of the represented venue.
        /// </summary>
        internal string Coordinates { get; }

        /// <summary>
        /// Gets the course
        /// </summary>
        /// <value>The course</value>
        internal ICollection<CourseDto> Courses { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VenueDto"/> class
        /// </summary>
        /// <param name="venue">A <see cref="venue"/> instance containing venue related information</param>
        internal VenueDto(venue venue)
            : base(venue.id, venue.name)
        {
            Capacity = venue.capacitySpecified
                           ? (int?)venue.capacity
                           : null;
            City = venue.city_name;
            Country = venue.country_name;
            CountryCode = venue.country_code;
            State = venue.state;
            Coordinates = venue.map_coordinates;
            Courses = venue.course.IsNullOrEmpty()
                          ? new List<CourseDto>()
                          : venue.course.Select(s => new CourseDto(s)).ToList();
        }
    }
}
