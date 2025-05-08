// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;

public static class SeasonEndpoints
{
    public static currentSeason GetVolleyballWorldChampionshipWomenSeason2025()
    {
        return new SeasonBuilder()
              .WithId(Urn.Parse("sr:season:129215"))
              .WithName("FiVB World Championship Women 2025")
              .WithTournamentId(Urn.Parse("sr:tournament:32"))
              .WithStartDate(new DateOnly(2025, 8, 22))
              .WithEndDate(new DateOnly(2025, 9, 7))
              .WithYear("2025")
              .BuildCurrent();
    }
}
