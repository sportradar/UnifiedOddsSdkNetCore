// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api;

public class ResultChangesEndpoint
{
    private readonly resultChangesEndpoint _resultChangesEndpoint = new resultChangesEndpoint();
    private readonly List<resultChange> _resultChanges = new List<resultChange>();

    public static ResultChangesEndpoint Create()
    {
        var fcEndpoint = new ResultChangesEndpoint
        {
            _resultChangesEndpoint =
                                     {
                                         generated_at = DateTime.Now,
                                         generated_atSpecified = true
                                     }
        };
        return fcEndpoint;
    }

    public ResultChangesEndpoint WithResultChange(resultChange resultChange)
    {
        _resultChanges.Add(resultChange);
        return this;
    }

    public ResultChangesEndpoint WithResultChange(Urn eventId, DateTime updated)
    {
        var change = new resultChange
        {
            sport_event_id = eventId.ToString(),
            update_time = updated
        };
        WithResultChange(change);
        return this;
    }

    public ResultChangesEndpoint WithResultChange(int nbrOfChanges, DateTime updated)
    {
        for (var i = 1; i <= nbrOfChanges; i++)
        {
            var eventId = Urn.Parse($"sr:match:{i}");
            var newUpdated = updated.AddSeconds(i);
            WithResultChange(eventId, newUpdated);
        }
        return this;
    }

    public resultChangesEndpoint Build()
    {
        _resultChangesEndpoint.result_change = _resultChanges.ToArray();
        return _resultChangesEndpoint;
    }
}
