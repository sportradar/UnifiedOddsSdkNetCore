// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Sportradar.OddsFeed.SDK.Tests.Common.MockApi;

public class WireMockMappingBuilder
{
    private readonly WireMockServer _server;

    public WireMockMappingBuilder(WireMockServer server)
    {
        _server = server;
    }

    public WireMockMappingBuilder AddDefaultMaps()
    {
        AddConfigurationMaps();

        return this;
    }
    public WireMockMappingBuilder AddConfigurationMaps()
    {
        var mappingName = "MyEndpointMapping";

        _server
            .Given(Request.Create().WithPath("/myapi").UsingGet())
            .WithTitle(mappingName) // Naming the mapping
            .RespondWith(Response.Create().WithBody("Initial response"));

        return this;
    }

    public WireMockMappingBuilder RemoveMap(string mappingName)
    {
        return this;
    }

    // private static IRespondWithAProvider CreateMapping(string path, string responseBody)
    // {
    //     return Request.Create().WithPath(path).UsingGet()
    //         .RespondWith(Response.Create().WithBody(responseBody));
    // }
}
