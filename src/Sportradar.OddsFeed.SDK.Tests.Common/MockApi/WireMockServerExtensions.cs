// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Sportradar.OddsFeed.SDK.Tests.Common.MockApi;

public static class WireMockServerExtensions
{
    public static WireMockMappingBuilder GetMappingBuilder(this WireMockServer wireMockServer)
    {
        return new WireMockMappingBuilder(wireMockServer);
    }

    public static void SetupAutoCalledEndpoints(this WireMockServer wireMockServer)
    {
        var mappingName = "MyEndpointMapping";

        wireMockServer
            .Given(Request.Create().WithPath("/myapi").UsingGet())
            .WithTitle(mappingName) // Naming the mapping
            .RespondWith(Response.Create().WithBody("Initial response"));
    }
}
