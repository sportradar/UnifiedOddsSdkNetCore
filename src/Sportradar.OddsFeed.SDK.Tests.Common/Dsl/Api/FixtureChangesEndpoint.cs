// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api;

public class FixtureChangesEndpoint
{
    private readonly fixtureChangesEndpoint _fixtureChangesEndpoint = new fixtureChangesEndpoint();
    private readonly List<fixtureChange> _fixtureChanges = new List<fixtureChange>();

    public static FixtureChangesEndpoint Create()
    {
        var fcEndpoint = new FixtureChangesEndpoint
        {
            _fixtureChangesEndpoint =
                                     {
                                         generated_at = DateTime.Now,
                                         generated_atSpecified = true
                                     }
        };
        return fcEndpoint;
    }

    public FixtureChangesEndpoint WithFixtureChange(fixtureChange fixtureChange)
    {
        _fixtureChanges.Add(fixtureChange);
        return this;
    }

    public FixtureChangesEndpoint WithFixtureChange(Urn eventId, DateTime updated)
    {
        var change = new fixtureChange
        {
            sport_event_id = eventId.ToString(),
            update_time = updated
        };
        WithFixtureChange(change);
        return this;
    }

    public FixtureChangesEndpoint WithFixtureChange(int nbrOfChanges, DateTime updated)
    {
        for (var i = 1; i <= nbrOfChanges; i++)
        {
            var eventId = Urn.Parse($"sr:match:{i}");
            var newUpdated = updated.AddSeconds(i);
            WithFixtureChange(eventId, newUpdated);
        }
        return this;
    }

    public fixtureChangesEndpoint Build()
    {
        _fixtureChangesEndpoint.fixture_change = _fixtureChanges.ToArray();
        return _fixtureChangesEndpoint;
    }
}
