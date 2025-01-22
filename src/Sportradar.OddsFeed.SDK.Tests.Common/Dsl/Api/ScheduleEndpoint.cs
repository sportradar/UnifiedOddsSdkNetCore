// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api;

public class ScheduleEndpoint
{
    private readonly scheduleEndpoint _scheduleEndpoint = new();
    private readonly List<sportEvent> _sportEvents = new();

    public static ScheduleEndpoint Create()
    {
        var fcEndpoint = new ScheduleEndpoint
        {
            _scheduleEndpoint =
                                     {
                                         generated_at = DateTime.Now,
                                         generated_atSpecified = true
                                     }
        };
        return fcEndpoint;
    }

    public ScheduleEndpoint AddSportEvents(int expectedCount)
    {
        for (var i = 0; i < expectedCount; i++)
        {
            _sportEvents.Add(BuildSportEvent(i));
        }

        return this;
    }

    private static sportEvent BuildSportEvent(int index)
    {
        return new sportEvent()
        {
            id = "sr:match:2222" + index,
            name = "mocked match for index " + index
        };
    }

    public scheduleEndpoint Build()
    {
        _scheduleEndpoint.sport_event = _sportEvents.ToArray();
        return _scheduleEndpoint;
    }
}
