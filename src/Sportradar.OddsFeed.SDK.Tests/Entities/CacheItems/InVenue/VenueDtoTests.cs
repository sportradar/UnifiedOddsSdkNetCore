using System;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InVenue;
public class VenueDtoTests : VenueHelper
{
    [Fact]
    public void Constructor_VenueIsNull_ThrowArgumentNullException()
    {
        Assert.Throws<NullReferenceException>(() => new VenueDto(null));
    }

    [Fact]
    public void Constructor_CapacitySpecifiedIsTrue_SetCapacityCorrectly()
    {
        var inputVenue = GenerateApiVenue();

        var dto = new VenueDto(inputVenue);

        Assert.Equal(5000, dto.Capacity);
    }

    [Fact]
    public void Constructor_CapacitySpecifiedIsFalse_SetCapacityToNull()
    {
        var inputVenue = GenerateApiVenue(capacitySpecified: false);

        var dto = new VenueDto(inputVenue);

        Assert.Null(dto.Capacity);
    }

    [Fact]
    public void Constructor_CityIsNotNull_SetCityCorrectly()
    {
        var inputVenue = GenerateApiVenue(cityIsNull: false);

        var dto = new VenueDto(inputVenue);

        Assert.Equal(DefaultVenueCityName, dto.City);
    }

    [Fact]
    public void Constructor_CityIsNull_SetCityToNull()
    {
        var inputVenue = GenerateApiVenue(cityIsNull: true);

        var dto = new VenueDto(inputVenue);

        Assert.Null(dto.City);
    }

    [Fact]
    public void Constructor_CountryIsNotNull_SetCorrectly()
    {
        var inputVenue = GenerateApiVenue(countryIsNull: false);

        var dto = new VenueDto(inputVenue);

        Assert.Equal(DefaultVenueCountryName, dto.Country);
    }

    [Fact]
    public void Constructor_CountryIsNull_SetToNull()
    {
        var inputVenue = GenerateApiVenue(countryIsNull: true);

        var dto = new VenueDto(inputVenue);

        Assert.Null(dto.Country);
    }

    [Fact]
    public void Constructor_CountryCodeIsNotNull_SetCorrectly()
    {
        var inputVenue = GenerateApiVenue(countryCodeIsNull: false);

        var dto = new VenueDto(inputVenue);

        Assert.Equal(DefaultVenueCountryCode, dto.CountryCode);
    }

    [Fact]
    public void Constructor_CountryCodeIsNull_SetToNull()
    {
        var inputVenue = GenerateApiVenue(countryCodeIsNull: true);

        var dto = new VenueDto(inputVenue);

        Assert.Null(dto.CountryCode);
    }

    [Fact]
    public void Constructor_StateIsNotNull_SetCorrectly()
    {
        var inputVenue = GenerateApiVenue(stateIsNull: false);

        var dto = new VenueDto(inputVenue);

        Assert.Equal(DefaultVenueStateName, dto.State);
    }

    [Fact]
    public void Constructor_StateIsNull_SetToNull()
    {
        var inputVenue = GenerateApiVenue(stateIsNull: true);

        var dto = new VenueDto(inputVenue);

        Assert.Null(dto.State);
    }

    [Fact]
    public void Constructor_CoordinatesIsNotNull_SetCorrectly()
    {
        var inputVenue = GenerateApiVenue(mapIsNull: false);

        var dto = new VenueDto(inputVenue);

        Assert.Equal(DefaultVenueCoordinates, dto.Coordinates);
    }

    [Fact]
    public void Constructor_CoordinatesIsNull_SetToNull()
    {
        var inputVenue = GenerateApiVenue(mapIsNull: true);

        var dto = new VenueDto(inputVenue);

        Assert.Null(dto.Coordinates);
    }

    [Fact]
    public void Constructor_CourseIsNull_SetCoursesToEmpty()
    {
        var inputVenue = GenerateApiVenue(courseSize: 0);

        var dto = new VenueDto(inputVenue);

        Assert.Empty(dto.Courses);
    }

    [Fact]
    public void Constructor_CourseIsNotNull_SetCoursesCorrectly()
    {
        var inputVenue = GenerateApiVenue(courseSize: 1);

        var dto = new VenueDto(inputVenue);

        Assert.Equal(inputVenue.course.Length, dto.Courses.Count);
    }

    [Fact]
    public void Constructor_CourseMultiple_SetCoursesCorrectly()
    {
        var inputVenue = GenerateApiVenue(courseSize: 10);

        var dto = new VenueDto(inputVenue);

        Assert.Equal(10, dto.Courses.Count);
    }
}
