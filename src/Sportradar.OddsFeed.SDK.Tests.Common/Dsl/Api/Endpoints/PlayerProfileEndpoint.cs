// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;

public class PlayerProfileEndpoint
{
    public static playerProfileEndpoint Build(Func<PlayerBuilder, PlayerBuilder> builderFunc)
    {
        return new playerProfileEndpoint
        {
            generated_at = DateTime.Now,
            generated_atSpecified = true,
            player = builderFunc(new PlayerBuilder()).BuildExtended()
        };
    }
}
