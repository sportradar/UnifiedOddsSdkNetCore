// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InVenue;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest.OutVenue;

public class VenueTests : VenueHelper
{
    [Fact]
    public void Constructor_CacheItemIsNull_ThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Venue(null, new List<CultureInfo> { TestData.Culture }));
    }

    [Fact]
    public void Constructor_CulturesIsNull_ThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Venue(GetSampleVenueCacheItem(), null));
    }

    [Fact]
    public void Constructor_CulturesIsEmpty_ThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Venue(GetSampleVenueCacheItem(), new List<CultureInfo>()));
    }

    [Fact]
    public void Constructor_ValidBaseParameters_InitializeCorrectly()
    {
        var venueCi = GetSampleVenueCacheItem();
        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.Culture });

        Assert.NotNull(venue);
        Assert.Equal(venueCi.Id, venue.Id);
        Assert.Single(venue.Names);
        Assert.Equal(venueCi.GetName(TestData.Culture), venue.Names[TestData.Culture]);
    }

    [Fact]
    public void Constructor_ValidAdditionalParameters_InitializeCorrectly()
    {
        var venueCi = GetSampleVenueCacheItem();
        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.Culture });

        Assert.Single(venue.Courses);
        Assert.Equal(9, venue.Courses.First().Holes.Count);
        Assert.Single(venue.Cities);
        Assert.Equal(venueCi.GetCity(TestData.Culture), venue.Cities[TestData.Culture]);
        Assert.Single(venue.Countries);
        Assert.Equal(venueCi.GetCountry(TestData.Culture), venue.Countries[TestData.Culture]);
        Assert.Equal(venueCi.CountryCode, venue.CountryCode);
        Assert.Equal(venueCi.State, venue.State);
        Assert.NotNull(venue.Capacity);
        Assert.Equal(venueCi.Capacity, venue.Capacity);
        Assert.Equal(venueCi.Coordinates, venue.Coordinates);
    }

    [Fact]
    public void Constructor_FullVenueCacheItemMultiCulture_SetPropertiesCorrectly()
    {
        var venueCi = new VenueCacheItem(new VenueDto(GenerateApiVenue(courseSize: 1)), TestData.Culture);
        var sapiVenue2 = GenerateApiVenue(courseSize: 1);
        sapiVenue2.name = "Second Name";
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.CultureNl);

        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.Culture, TestData.CultureNl });

        Assert.NotNull(venue);
        Assert.Equal(venueCi.Id, venue.Id);
        Assert.Equal(2, venue.Names.Count);
        Assert.Equal(venueCi.GetName(TestData.Culture), venue.Names[TestData.Culture]);
        Assert.Equal(venueCi.GetName(TestData.CultureNl), venue.Names[TestData.CultureNl]);
        Assert.Single(venue.Courses);
        Assert.Equal(2, venue.Names.Count);
        Assert.Equal(9, venue.Courses.First().Holes.Count);
    }

    [Fact]
    public void Constructor_FullVenueCacheItemForOneCulture_SetPropertiesCorrectly()
    {
        var venueCi = new VenueCacheItem(new VenueDto(GenerateApiVenue(courseSize: 1)), TestData.Culture);
        var sapiVenue2 = GenerateApiVenue(courseSize: 1);
        sapiVenue2.name = "Second Name";
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.CultureNl);

        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.CultureNl });

        Assert.NotNull(venue);
        Assert.Equal(venueCi.Id, venue.Id);
        Assert.Single(venue.Names);
        Assert.Equal(venueCi.GetName(TestData.CultureNl), venue.Names[TestData.CultureNl]);
        Assert.Single(venue.Courses);
        Assert.Equal(9, venue.Courses.First().Holes.Count);
    }

    [Fact]
    public void Constructor_VenueCacheItemWithoutName_SetPropertiesCorrectly()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 1);
        sapiVenue.name = null;
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);

        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.Culture });

        Assert.NotNull(venue);
        Assert.Single(venue.Names);
        Assert.Null(venue.Names[TestData.Culture]);
    }

    [Fact]
    public void Constructor_VenueCacheItemWithoutCityName_SetPropertiesCorrectly()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 1);
        sapiVenue.city_name = null;
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);

        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.Culture });

        Assert.NotNull(venue);
        Assert.Single(venue.Cities);
        Assert.Null(venue.Cities[TestData.Culture]);
    }

    [Fact]
    public void Constructor_VenueCacheItemWithoutCountryName_SetPropertiesCorrectly()
    {
        var sapiVenue = GenerateApiVenue(courseSize: 1);
        sapiVenue.country_name = null;
        var venueCi = new VenueCacheItem(new VenueDto(sapiVenue), TestData.Culture);

        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.Culture });

        Assert.NotNull(venue);
        Assert.Single(venue.Countries);
        Assert.Null(venue.Countries[TestData.Culture]);
    }

    [Fact]
    public void GetName_CultureExists_ReturnsName()
    {
        var venueCi = GetSampleVenueCacheItem();
        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.Culture });

        Assert.Equal(venueCi.GetName(TestData.Culture), venue.GetName(TestData.Culture));
    }

    [Fact]
    public void GetName_MultipleCulturesExists_ReturnsName()
    {
        var venueCi = GetSampleVenueCacheItem();
        var sapiVenue2 = GenerateApiVenue(courseSize: 1);
        sapiVenue2.name = "Second Name";
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.CultureNl);
        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.Culture, TestData.CultureNl });

        Assert.Equal(venueCi.GetName(TestData.Culture), venue.GetName(TestData.Culture));
        Assert.Equal(venueCi.GetName(TestData.CultureNl), venue.GetName(TestData.CultureNl));
        Assert.NotEqual(venue.GetName(TestData.Culture), venue.GetName(TestData.CultureNl));
    }

    [Fact]
    public void GetName_CultureDoesNotExist_ReturnsNull()
    {
        var venue = new Venue(GetSampleVenueCacheItem(), new List<CultureInfo> { TestData.Culture });

        var result = venue.GetName(TestData.CultureNl);

        Assert.Null(result);
    }

    [Fact]
    public void GetCity_CultureExists_ReturnsName()
    {
        var venueCi = GetSampleVenueCacheItem();
        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.Culture });

        Assert.Equal(venueCi.GetCity(TestData.Culture), venue.GetCity(TestData.Culture));
    }

    [Fact]
    public void GetCity_MultipleCulturesExists_ReturnsName()
    {
        var venueCi = GetSampleVenueCacheItem();
        var sapiVenue2 = GenerateApiVenue(courseSize: 1);
        sapiVenue2.city_name = "Second Name";
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.CultureNl);
        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.Culture, TestData.CultureNl });

        Assert.Equal(venueCi.GetCity(TestData.Culture), venue.GetCity(TestData.Culture));
        Assert.Equal(venueCi.GetCity(TestData.CultureNl), venue.GetCity(TestData.CultureNl));
        Assert.NotEqual(venue.GetCity(TestData.Culture), venue.GetCity(TestData.CultureNl));
    }

    [Fact]
    public void GetCity_CultureDoesNotExist_ReturnsNull()
    {
        var venue = new Venue(GetSampleVenueCacheItem(), new List<CultureInfo> { TestData.Culture });

        var result = venue.GetCity(TestData.CultureNl);

        Assert.Null(result);
    }

    [Fact]
    public void GetCountry_CultureExists_ReturnsName()
    {
        var venueCi = GetSampleVenueCacheItem();
        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.Culture });

        Assert.Equal(venueCi.GetCountry(TestData.Culture), venue.GetCountry(TestData.Culture));
    }

    [Fact]
    public void GetCountry_MultipleCulturesExists_ReturnsName()
    {
        var venueCi = GetSampleVenueCacheItem();
        var sapiVenue2 = GenerateApiVenue(courseSize: 1);
        sapiVenue2.country_name = "Second Name";
        venueCi.Merge(new VenueDto(sapiVenue2), TestData.CultureNl);
        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.Culture, TestData.CultureNl });

        Assert.Equal(venueCi.GetCountry(TestData.Culture), venue.GetCountry(TestData.Culture));
        Assert.Equal(venueCi.GetCountry(TestData.CultureNl), venue.GetCountry(TestData.CultureNl));
        Assert.NotEqual(venue.GetCountry(TestData.Culture), venue.GetCountry(TestData.CultureNl));
    }

    [Fact]
    public void GetCountry_CultureDoesNotExist_ReturnsNull()
    {
        var venue = new Venue(GetSampleVenueCacheItem(), new List<CultureInfo> { TestData.Culture });

        var result = venue.GetCountry(TestData.CultureNl);

        Assert.Null(result);
    }

    [Fact]
    public void PrintI_ReturnsExpectedString()
    {
        var venueCi = GetSampleVenueCacheItem();
        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.Culture });

        var result = venue.ToString("I");

        Assert.Equal($"Id={venueCi.Id}", result);
    }

    [Fact]
    public void PrintC_ReturnsExpectedString()
    {
        var venueCi = GetSampleVenueCacheItem();
        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.Culture });

        var result = venue.ToString("C");

        Assert.Contains($"Id={venue.Id},", result);
        Assert.Contains($"Name[{venue.Names.Keys.First().TwoLetterISOLanguageName}]={venue.Names.Values.First()}", result);
    }

    [Fact]
    public void PrintF_ReturnsExpectedString()
    {
        var venueCi = GetSampleVenueCacheItem();
        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.Culture });

        var result = venue.ToString("F");

        Assert.Contains($"Id={venueCi.Id},", result);
        Assert.Contains("Names=[", result);
        Assert.Contains("Holes=[", result);
    }

    [Fact]
    public void PrintJ_ValidCourse_ReturnsCorrectString()
    {
        var venueCi = GetSampleVenueCacheItem();
        var venue = new Venue(venueCi, new List<CultureInfo> { TestData.Culture });

        Assert.Throws<InvalidDataContractException>(() => venue.ToString("J"));
    }

    private VenueCacheItem GetSampleVenueCacheItem()
    {
        var venueCacheItem = new VenueCacheItem(new VenueDto(GenerateApiVenue(courseSize: 1)), TestData.Culture);
        return venueCacheItem;
    }
}
