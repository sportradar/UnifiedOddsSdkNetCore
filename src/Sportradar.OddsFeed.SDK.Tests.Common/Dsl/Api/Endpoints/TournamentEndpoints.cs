// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;

public static class TournamentEndpoints
{
    public static tournament VolleyballWorldChampionshipWomenTournament()
    {
        return new TournamentBuilder()
              .WithId(Urn.Parse("sr:tournament:32"))
              .WithName("World Championship, Women")
              .WithSport(Urn.Parse("sr:sport:23"), "Volleyball")
              .WithCategory(Urn.Parse("sr:category:136"), "International", null)
              .Build();
    }
}
