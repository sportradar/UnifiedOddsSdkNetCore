// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Extensions;

internal static class MatchSummaryEndpointExtensions
{
    public static Urn GetMatchId(this matchSummaryEndpoint matchSummaryEndpoint)
    {
        return Urn.Parse(matchSummaryEndpoint.sport_event.id);
    }

    public static Urn GetSportId(this matchSummaryEndpoint matchSummaryEndpoint)
    {
        return Urn.Parse(matchSummaryEndpoint.sport_event.tournament.sport.id);
    }

    public static Uri GetMatchSummaryUri(this matchSummaryEndpoint matchSummaryEndpoint, string baseUrl, CultureInfo language)
    {
        return new Uri($"{baseUrl}/v1/sports/{language.TwoLetterISOLanguageName}/sport_events/{matchSummaryEndpoint.sport_event.id}/summary.xml");
    }
}
