// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;

public class DrawSummaryEndpoint
{
    private draw_fixture _drawFixture;
    private draw_result _drawResult;
    private DateTime? _generatedAt;

    public DrawSummaryEndpoint WithDrawFixture(Func<DrawFixtureBuilder, DrawFixtureBuilder> builder)
    {
        _drawFixture = builder(new DrawFixtureBuilder()).Build();
        return this;
    }

    public DrawSummaryEndpoint WithDrawResult(Func<DrawResultBuilder, DrawResultBuilder> builder)
    {
        _drawResult = builder(new DrawResultBuilder()).Build();
        return this;
    }

    public DrawSummaryEndpoint WithGeneratedAt(DateTime generatedAt)
    {
        _generatedAt = generatedAt;
        return this;
    }

    public draw_summary Build()
    {
        var summary = new draw_summary
        {
            draw_fixture = _drawFixture,
            draw_result = _drawResult
        };
        if (_generatedAt.HasValue)
        {
            summary.generated_at = _generatedAt.Value;
            summary.generated_atSpecified = true;
        }

        return summary;
    }
}
