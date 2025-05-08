// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <venue id="sr:venue:2010" name="SAP Arena" capacity="13200" city_name="Mannheim" country_name="Germany" country_code="DEU" map_coordinates="49.46373,8.51757"/>
public class VenueBuilder
{
    private readonly venue _venue = new venue();

    public VenueBuilder WithId(Urn id)
    {
        _venue.id = id.ToString();
        return this;
    }

    public VenueBuilder WithName(string name)
    {
        _venue.name = name;
        return this;
    }

    public VenueBuilder WithCapacity(int capacity)
    {
        _venue.capacity = capacity;
        _venue.capacitySpecified = true;
        return this;
    }

    public VenueBuilder WithCityName(string cityName)
    {
        _venue.city_name = cityName;
        return this;
    }

    public VenueBuilder WithCountryName(string countryName)
    {
        _venue.country_name = countryName;
        return this;
    }

    public VenueBuilder WithCountryCode(string countryCode)
    {
        _venue.country_code = countryCode;
        return this;
    }

    public VenueBuilder WithMapCoordinates(double latitude, double longitude)
    {
        _venue.map_coordinates = $"{latitude},{longitude}";
        return this;
    }

    public VenueBuilder WithCourses(params course[] courses)
    {
        _venue.course = courses;
        return this;
    }

    public venue Build()
    {
        return _venue;
    }
}
