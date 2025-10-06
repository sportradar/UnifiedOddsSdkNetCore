// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

public sealed class MatchTimelineEndpointBuilder
{
    private sportEvent _sportEvent;
    private sportEventConditions _sportEventConditions;
    private restSportEventStatus _sportEventStatus;
    private coverageInfo _coverageInfo;
    private readonly List<basicEvent> _timeline = [];

    private DateTime? _generatedAt;
    private bool? _generatedAtSpecified;

    private MatchTimelineEndpointBuilder() { }

    public static MatchTimelineEndpointBuilder Create()
    {
        return new MatchTimelineEndpointBuilder();
    }

    public MatchTimelineEndpointBuilder WithGeneratedAtUtc(DateTime generatedAtUtc)
    {
        _generatedAt = generatedAtUtc;
        _generatedAtSpecified = true;
        return this;
    }

    public MatchTimelineEndpointBuilder WithSportEvent(sportEvent value)
    {
        _sportEvent = value;
        return this;
    }

    public MatchTimelineEndpointBuilder ConfigureSportEvent(Action<sportEvent> configure)
    {
        if (configure == null)
        {
            return this;
        }

        _sportEvent ??= new sportEvent();

        configure(_sportEvent);
        return this;
    }

    public MatchTimelineEndpointBuilder WithSportEventConditions(sportEventConditions value)
    {
        _sportEventConditions = value;
        return this;
    }

    public MatchTimelineEndpointBuilder WithSportEventStatus(restSportEventStatus value)
    {
        _sportEventStatus = value;
        return this;
    }

    public MatchTimelineEndpointBuilder ConfigureSportEventStatus(Action<restSportEventStatus> configure)
    {
        if (configure == null)
        {
            return this;
        }

        _sportEventStatus ??= new restSportEventStatus();

        configure(_sportEventStatus);
        return this;
    }

    public MatchTimelineEndpointBuilder WithCoverageInfo(coverageInfo value)
    {
        _coverageInfo = value;
        return this;
    }

    public MatchTimelineEndpointBuilder WithConfigureCoverageInfo(coverageInfo coverageInfo)
    {
        _coverageInfo = coverageInfo;
        return this;
    }

    public MatchTimelineEndpointBuilder SetTimeline(IEnumerable<basicEvent> timelineEvents)
    {
        _timeline.Clear();
        if (timelineEvents != null)
        {
            _timeline.AddRange(timelineEvents);
        }

        return this;
    }

    public MatchTimelineEndpointBuilder AddEvent(basicEvent timelineEvent)
    {
        if (timelineEvent != null)
        {
            _timeline.Add(timelineEvent);
        }

        return this;
    }

    public MatchTimelineEndpointBuilder AddEvent(Action<basicEvent> configure)
    {
        if (configure == null)
        {
            return this;
        }

        var timelineEvent = new basicEvent();
        configure(timelineEvent);
        _timeline.Add(timelineEvent);
        return this;
    }

    public MatchTimelineEndpointBuilder EditTimeline(Action<List<basicEvent>> edit)
    {
        edit?.Invoke(_timeline);

        return this;
    }

    public matchTimelineEndpoint Build()
    {
        var endpoint = new matchTimelineEndpoint
        {
            sport_event = _sportEvent,
            sport_event_conditions = _sportEventConditions,
            sport_event_status = _sportEventStatus,
            coverage_info = _coverageInfo,
            timeline = _timeline?.ToArray() ?? []
        };

        if (_generatedAt.HasValue)
        {
            endpoint.generated_at = _generatedAt.Value;
            endpoint.generated_atSpecified = true;
        }
        else
        {
            endpoint.generated_at = default;
            endpoint.generated_atSpecified = false;
        }

        return endpoint;
    }
}
