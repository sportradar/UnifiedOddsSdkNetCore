// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

public class MatchStatusDescriptionsBuilder
{
    private readonly List<desc_match_status> _statuses = new();
    private response_code _responseCode;
    private bool _responseCodeSpecified;
    private string _location;

    public MatchStatusDescriptionsBuilder WithResponseCode(response_code responseCode)
    {
        _responseCode = responseCode;
        _responseCodeSpecified = true;
        return this;
    }

    public MatchStatusDescriptionsBuilder WithLocation(string location)
    {
        _location = location;
        return this;
    }

    public MatchStatusDescriptionsBuilder AddMatchStatus(Func<DescMatchStatusBuilder, DescMatchStatusBuilder> builderFunc)
    {
        var builder = builderFunc(new DescMatchStatusBuilder());
        _statuses.Add(builder.Build());
        return this;
    }

    public match_status_descriptions Build()
    {
        return new match_status_descriptions
        {
            response_code = _responseCode,
            response_codeSpecified = _responseCodeSpecified,
            location = _location,
            match_status = _statuses.ToArray()
        };
    }
}
