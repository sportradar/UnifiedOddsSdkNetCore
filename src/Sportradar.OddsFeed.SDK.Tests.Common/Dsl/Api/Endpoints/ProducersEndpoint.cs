// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;

public class ProducersEndpoint
{
    public static producers GetOneProducer(int producerId = 1)
    {
        return new producers
        {
            location = "some-location",
            producer =
            [
                new producer
                {
                    id = producerId,
                    name = "LO",
                    description = "Live Odds",
                    api_url = "https://localhost/v1/liveodds/",
                    active = true,
                    scope = "live",
                    stateful_recovery_window_in_minutes = 600
                }
            ]
        };
    }
}
