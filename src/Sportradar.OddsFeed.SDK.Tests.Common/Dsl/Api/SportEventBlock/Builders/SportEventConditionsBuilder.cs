// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <sport_event_conditions>
//   <venue id="sr:venue:2010" name="SAP Arena" capacity="13200" city_name="Mannheim" country_name="Germany" country_code="DEU" map_coordinates="49.46373,8.51757"/>
// </sport_event_conditions>
public class SportEventConditionsBuilder
{
    private readonly sportEventConditions _sportEventConditions = new sportEventConditions();

    public SportEventConditionsBuilder WithVenue(Func<VenueBuilder, VenueBuilder> venueFunc)
    {
        var venue = venueFunc(new VenueBuilder()).Build();
        _sportEventConditions.venue = venue;
        return this;
    }

    public SportEventConditionsBuilder WithVenue(venue venue)
    {
        _sportEventConditions.venue = venue;
        return this;
    }

    public SportEventConditionsBuilder WithWeatherInfo(Func<WeatherInfoBuilder, WeatherInfoBuilder> weatherFunc)
    {
        var weather = weatherFunc(new WeatherInfoBuilder()).Build();
        _sportEventConditions.weather_info = weather;
        return this;
    }

    public SportEventConditionsBuilder WithReferee(Urn refereeId, string name, string nationality)
    {
        _sportEventConditions.referee = new referee
        {
            id = refereeId.ToString(),
            name = name,
            nationality = nationality
        };
        return this;
    }

    public sportEventConditions Build()
    {
        return _sportEventConditions;
    }
}
