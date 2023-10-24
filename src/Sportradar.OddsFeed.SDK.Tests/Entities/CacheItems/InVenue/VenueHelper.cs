using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InVenue;

public class VenueHelper
{
    private const string DefaultVenueIdString = "sr:venue:589";
    protected readonly Urn DefaultCourseId = Urn.Parse("sr:venue:123");
    protected const string DefaultCourseName = "some-course-name";
    public readonly Urn DefaultVenueId = Urn.Parse(DefaultVenueIdString);
    public const string DefaultVenueName = "some-venue-name";

    public const string DefaultVenueCityName = "some-default-venue-city-name";
    public const string DefaultVenueCountryName = "some-default-venue-country-name";
    public const string DefaultVenueCountryCode = "CC";
    public const string DefaultVenueStateName = "some-default-venue-state-name";
    public const string DefaultVenueCoordinates = "50.123, 20.321";

    public static course GetApiCourseFull(int holeSize = 9, int courseId = 123)
    {
        return new course { id = Urn.Parse($"sr:venue:{courseId}").ToString(), name = DefaultCourseName, hole = GetApiHoles(holeSize) };
    }

    public course GetApiCourseWithoutId(int holeSize = 9)
    {
        return new course { name = DefaultCourseName, hole = GetApiHoles(holeSize) };
    }

    public course GetApiCourseWithoutName(int holeSize = 9)
    {
        return new course { id = DefaultCourseId.ToString(), hole = GetApiHoles(holeSize) };
    }

    public course GetApiCourseWithoutHoles()
    {
        return new course { id = DefaultCourseId.ToString(), name = DefaultCourseName };
    }

    public static hole[] GetApiHoles(int holeSize = 9)
    {
        var holes = new List<hole>();
        for (var i = 1; i <= holeSize; i++)
        {
            holes.Add(new hole { number = i, par = 4 });
        }
        return holes.ToArray();
    }

    internal void ValidateHoles(ICollection<HoleCacheItem> holes, int holeSize = 9)
    {
        Assert.Equal(holeSize, holes.Count);
        for (var i = 1; i <= holeSize; i++)
        {
            Assert.Equal(i, holes.ElementAt(i - 1).Number);
            Assert.Equal(4, holes.ElementAt(i - 1).Par);
        }
    }

    public ICollection<ExportableHole> GetExportableHoles(int holeSize = 9)
    {
        var holes = new List<ExportableHole>();
        for (var i = 1; i <= holeSize; i++)
        {
            holes.Add(new ExportableHole { Number = i, Par = 4 });
        }
        return holes.ToArray();
    }

    public static course[] GetApiCourses(int courseSize = 1, int holeSize = 9)
    {
        var courses = new List<course>();
        for (var i = 1; i <= courseSize; i++)
        {
            var courseId = 122 + i;
            courses.Add(GetApiCourseFull(holeSize, courseId));
        }
        return courses.ToArray();
    }

    public course[] GetApiCoursesWithName(CultureInfo culture, int courseSize = 1, int holeSize = 9)
    {
        var courses = new List<course>();
        for (var i = 1; i <= courseSize; i++)
        {
            var courseId = i;
            var course = GetApiCourseFull(holeSize, courseId);
            course.name = $"Name for {DefaultVenueId.Id} [{culture.TwoLetterISOLanguageName}]";
            courses.Add(course);
        }
        return courses.ToArray();
    }

    public static venue GenerateApiVenue(
        bool capacitySpecified = true,
        bool cityIsNull = false,
        bool countryIsNull = false,
        bool countryCodeIsNull = false,
        bool stateIsNull = false,
        bool mapIsNull = false,
        int courseSize = 0)
    {
        return new venue
        {
            id = DefaultVenueIdString,
            name = DefaultVenueName,
            capacity = capacitySpecified ? 5000 : 0,
            capacitySpecified = capacitySpecified,
            city_name = cityIsNull ? null : DefaultVenueCityName,
            country_name = countryIsNull ? null : DefaultVenueCountryName,
            country_code = countryCodeIsNull ? null : DefaultVenueCountryCode,
            state = stateIsNull ? null : DefaultVenueStateName,
            map_coordinates = mapIsNull ? null : DefaultVenueCoordinates,
            course = courseSize == 0 ? null : GetApiCourses(courseSize)
        };
    }

    public venue GenerateApiVenue(CultureInfo culture, int courseSize = 0)
    {
        return new venue
        {
            id = DefaultVenueId.ToString(),
            name = $"Name for {DefaultVenueId.Id} [{culture.TwoLetterISOLanguageName}]",
            capacity = 5000,
            capacitySpecified = true,
            city_name = DefaultVenueCityName,
            country_name = DefaultVenueCountryName,
            country_code = DefaultVenueCountryCode,
            state = DefaultVenueStateName,
            map_coordinates = DefaultVenueCoordinates,
            course = courseSize == 0 ? null : GetApiCoursesWithName(culture, courseSize)
        };
    }

    internal void ValidateVenueCi(venue sapiVenue, VenueCacheItem venueCi, CultureInfo culture)
    {
        Assert.Equal(Urn.Parse(sapiVenue.id), venueCi.Id);
        Assert.Equal(sapiVenue.name, venueCi.GetName(culture));
        Assert.Equal(sapiVenue.city_name, venueCi.GetCity(culture));
        Assert.Equal(sapiVenue.country_name, venueCi.GetCountry(culture));
        Assert.Equal(sapiVenue.country_code, venueCi.CountryCode);
        Assert.Equal(sapiVenue.state, venueCi.State);
        if (sapiVenue.capacitySpecified)
        {
            Assert.Equal(sapiVenue.capacity, venueCi.Capacity);
        }
        else
        {
            Assert.Equal(0, venueCi.Capacity);
        }

        if (sapiVenue.course.IsNullOrEmpty())
        {
            Assert.Empty(venueCi.Courses);
        }
        else
        {
            Assert.Equal(sapiVenue.course.Length, venueCi.Courses.Count);
            foreach (var course in sapiVenue.course)
            {
                var courseCi = venueCi.Courses.First(c => c.Id.Equals(Urn.Parse(course.id)));
                Assert.NotNull(courseCi);
                Assert.Equal(course.name, courseCi.Names[culture]);
                Assert.Equal(course.hole.Length, courseCi.Holes.Count);
            }
        }
    }
}
