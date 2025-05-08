// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Extensions;

internal static class TournamentInfoEndpointExtensions
{
    public static Urn GetTournamentId(this tournamentInfoEndpoint tournamentInfoEndpoint)
    {
        return Urn.Parse(tournamentInfoEndpoint.tournament.id);
    }

    public static Urn GetSportId(this tournamentInfoEndpoint tournamentInfoEndpoint)
    {
        return Urn.Parse(tournamentInfoEndpoint.tournament.sport.id);
    }

    public static Uri GetTournamentSummaryUri(this tournamentInfoEndpoint tournamentInfoEndpoint, string baseUrl, CultureInfo language)
    {
        return new Uri($"{baseUrl}/v1/sports/{language.TwoLetterISOLanguageName}/sport_events/{tournamentInfoEndpoint.tournament.id}/summary.xml");
    }
}
