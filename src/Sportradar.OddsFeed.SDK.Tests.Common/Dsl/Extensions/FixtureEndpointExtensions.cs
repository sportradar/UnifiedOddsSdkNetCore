// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Extensions;

internal static class FixtureEndpointExtensions
{
    public static Urn GetMatchId(this fixturesEndpoint fixturesEndpoint)
    {
        return Urn.Parse(fixturesEndpoint.fixture.id);
    }

    public static Urn GetSportId(this fixturesEndpoint fixturesEndpoint)
    {
        return Urn.Parse(fixturesEndpoint.fixture.tournament.sport.id);
    }

    public static Uri GetFixtureUri(this fixturesEndpoint fixturesEndpoint, string baseUrl, CultureInfo language)
    {
        return new Uri($"{baseUrl}/v1/sports/{language.TwoLetterISOLanguageName}/sport_events/{fixturesEndpoint.fixture.id}/fixture.xml");
    }
}
