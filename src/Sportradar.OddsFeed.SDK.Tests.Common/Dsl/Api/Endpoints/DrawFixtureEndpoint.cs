// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;

public class DrawFixtureEndpoint
{
    private draw_fixture _drawFixture;
    private DateTime? _generatedAt;

    public DrawFixtureEndpoint WithDrawFixture(Func<DrawFixtureBuilder, DrawFixtureBuilder> builder)
    {
        _drawFixture = builder(new DrawFixtureBuilder()).Build();
        return this;
    }

    public DrawFixtureEndpoint WithGeneratedAt(DateTime generatedAt)
    {
        _generatedAt = generatedAt;
        return this;
    }

    public draw_fixtures Build()
    {
        var fixtures = new draw_fixtures
        {
            draw_fixture = _drawFixture
        };
        if (_generatedAt.HasValue)
        {
            fixtures.generated_at = _generatedAt.Value;
            fixtures.generated_atSpecified = true;
        }

        return fixtures;
    }
}
