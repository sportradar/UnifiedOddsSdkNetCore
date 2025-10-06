// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using SR = Sportradar.OddsFeed.SDK.Tests.Common.StaticRandom;

// ReSharper disable UnusedMember.Local

namespace Sportradar.OddsFeed.SDK.Tests.Common;

/// <summary>
/// Class used to manually create REST entities
/// </summary>
public class MessageFactoryFeed
{
    public static alive GetAlive(IProducer producer, bool subscribed = true)
    {
        return new alive
        {
            product = producer.Id,
            timestamp = GetTimestamp(),
            subscribed = subscribed ? 1 : 0
        };
    }

    public static snapshot_complete GetSnapshotComplete(IProducer producer, long requestId)
    {
        return new snapshot_complete
        {
            product = producer.Id,
            timestamp = GetTimestamp(),
            request_id = requestId
        };
    }

    public static odds_change GetOddsChange(IProducer producer, int sportId, int eventId, long requestId)
    {
        return new odds_change
        {
            product = producer.Id,
            timestamp = GetTimestamp(),
            SportId = sportId == 0 ? SR.Urn("sport") : SR.Urn(sportId, "sport"),
            EventUrn = SR.Urn(eventId, "match"),
            event_id = SR.Urn(eventId, "match").ToString(),
            request_id = requestId,
            request_idSpecified = requestId > 0
        };
    }

    public static bet_cancel GetBetCancel(IProducer producer, int eventId, long requestId)
    {
        return new bet_cancel
        {
            product = producer.Id,
            timestamp = GetTimestamp(),
            EventUrn = SR.Urn(eventId, "match"),
            event_id = SR.Urn(eventId, "match").ToString(),
            request_id = requestId,
            request_idSpecified = requestId > 0,
            market = GetMarkets(SR.I100).ToArray()
            // missing
        };
    }

    public static rollback_bet_cancel GetRollbackBetCancel(IProducer producer, int eventId, long requestId)
    {
        return new rollback_bet_cancel
        {
            product = producer.Id,
            timestamp = GetTimestamp(),
            EventUrn = SR.Urn(eventId, "match"),
            event_id = SR.Urn(eventId, "match").ToString(),
            request_id = requestId,
            request_idSpecified = requestId > 0
            // missing
        };
    }

    public static rollback_bet_settlement GetRollbackBetSettlement(IProducer producer, int eventId, long requestId)
    {
        return new rollback_bet_settlement
        {
            product = producer.Id,
            timestamp = GetTimestamp(),
            EventUrn = SR.Urn(eventId, "match"),
            event_id = SR.Urn(eventId, "match").ToString(),
            request_id = requestId,
            request_idSpecified = requestId > 0
            // missing
        };
    }

    public static bet_settlement GetBetSettlement(IProducer producer, int eventId, long requestId)
    {
        return new bet_settlement
        {
            product = producer.Id,
            timestamp = GetTimestamp(),
            EventUrn = SR.Urn(eventId, "match"),
            event_id = SR.Urn(eventId, "match").ToString(),
            request_id = requestId,
            request_idSpecified = requestId > 0
            // missing
        };
    }

    public static fixture_change GetFixtureChange(IProducer producer, int eventId, long requestId)
    {
        return new fixture_change
        {
            product = producer.Id,
            timestamp = GetTimestamp(),
            EventUrn = SR.Urn(eventId, "match"),
            event_id = SR.Urn(eventId, "match").ToString(),
            request_id = requestId,
            request_idSpecified = requestId > 0
            // missing
        };
    }

    private static long GetTimestamp()
    {
        return DateTime.Now.Ticks;
    }

    private static market GetMarket(int id, string specifiers)
    {
        return new market
        {
            id = id,
            specifiers = specifiers
        };
    }

    public static List<Urn> GetUrns(int count, string type)
    {
        var items = new List<Urn>();
        for (var i = 0; i < count; i++)
        {
            items.Add(SR.Urn(type));
        }
        return items;
    }

    public static List<market> GetMarkets(int count)
    {
        var items = new List<market>();
        for (var j = 0; j < count; j++)
        {
            items.Add(GetMarket(SR.I1000, SR.S1000));
        }
        return items;
    }
}
