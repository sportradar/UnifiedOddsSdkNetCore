// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;

public static class FixtureEndpoint
{
    public static fixturesEndpoint Raw(string matchId)
    {
        return new FixtureBuilder()
              .WithId(Urn.Parse(matchId))
              .WithStartTime(DateTime.Now.AddDays(-1))
              .Build();
    }

    public static fixturesEndpoint GetFixtureWithReference(string matchId, string referenceKey, string referenceValue)
    {
        var fixture = Raw(matchId);
        fixture.fixture.reference_ids =
            [
                new referenceIdsReference_id { name = referenceKey, value = referenceValue }
            ];
        return fixture;
    }

    public static fixturesEndpoint GetFixtureWithEmptyReferences(string matchId)
    {
        var fixture = Raw(matchId);
        fixture.fixture.name = "fixture-without-reference";
        fixture.fixture.reference_ids = [];
        return fixture;
    }
}

