// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Feed;

/// <summary>
/// Class for building test feed messages
/// </summary>
public class FeedMessageBuilder
{
    private readonly int _producerId;

    public FeedMessageBuilder(int producerId)
    {
        _producerId = producerId;
    }

    public static FeedMessageBuilder Create(int producerId)
    {
        return new FeedMessageBuilder(producerId);
    }

    /// <summary>
    /// Builds odds_change message
    /// </summary>
    /// <param name="eventId">The event id to be set (if null id=1000, if -1 id=Random, or eventId)</param>
    /// <param name="product">The product id message belongs to</param>
    /// <param name="requestId">The request id or null</param>
    /// <param name="timestamp">Timestamp to be applied or DateTime.Now</param>
    /// <returns>New odds_change message</returns>
    public odds_change BuildOddsChange(long? eventId = null, int? product = null, long? requestId = null, DateTime? timestamp = null)
    {
        if (eventId is -1)
        {
            eventId = StaticRandom.I();
        }

        return new odds_change
        {
            event_id = eventId == null ? "sr:match:1000" : $"sr:match:{eventId}",
            product = product ?? _producerId,
            timestamp = SdkInfo.ToEpochTime(timestamp ?? TimeProviderAccessor.Current.Now),
            request_id = requestId ?? 0
        };
    }

    public fixture_change BuildFixtureChange(long? eventId = null, int? product = null, long? requestId = null, DateTime? timestamp = null)
    {
        if (eventId is -1)
        {
            eventId = StaticRandom.I();
        }
        return new fixture_change { event_id = eventId == null ? "sr:match:1000" : $"sr:match:{eventId}", product = product ?? _producerId, timestamp = SdkInfo.ToEpochTime(timestamp ?? TimeProviderAccessor.Current.Now), request_id = requestId ?? 0 };
    }

    /// <summary>
    /// Builds bet_stop message
    /// </summary>
    /// <param name="eventId">The event id to be set (if null id=1000, if -1 id=Random, or eventId)</param>
    /// <param name="product">The product id message belongs to</param>
    /// <param name="requestId">The request id or null</param>
    /// <param name="timestamp">Timestamp to be applied or DateTime.Now</param>
    /// <returns>New bet_stop message</returns>
    public bet_stop BuildBetStop(long? eventId = null, int? product = null, long? requestId = null, DateTime? timestamp = null)
    {
        if (eventId is -1)
        {
            eventId = StaticRandom.I();
        }

        return new bet_stop
        {
            event_id = eventId == null ? "sr:match:1000" : $"sr:match:{eventId}",
            product = product ?? _producerId,
            timestamp = SdkInfo.ToEpochTime(timestamp ?? TimeProviderAccessor.Current.Now),
            request_id = requestId ?? 0
        };
    }

    /// <summary>
    /// Builds alive message
    /// </summary>
    /// <param name="product">The product id message belongs to</param>
    /// <param name="timestamp">Timestamp to be applied or DateTime.Now</param>
    /// <param name="subscribed">If subscribed attributed is 1 or 0</param>
    /// <returns>New alive message</returns>
    public alive BuildAlive(int? product = null, DateTime? timestamp = null, bool subscribed = true)
    {
        return new alive { product = product ?? _producerId, subscribed = subscribed ? 1 : 0, timestamp = SdkInfo.ToEpochTime(timestamp ?? TimeProviderAccessor.Current.Now) };
    }

    /// <summary>
    /// Builds bet_stop message
    /// </summary>
    /// <param name="requestId">The request id or null</param>
    /// <param name="product">The product id message belongs to</param>
    /// <param name="timestamp">Timestamp to be applied or DateTime.Now</param>
    /// <returns>New bet_stop message</returns>
    public snapshot_complete BuildSnapshotComplete(long requestId, int? product = null, DateTime? timestamp = null)
    {
        return new snapshot_complete { product = product ?? _producerId, timestamp = SdkInfo.ToEpochTime(timestamp ?? TimeProviderAccessor.Current.Now), request_id = requestId };
    }

    /// <summary>
    /// Builds the XML message body from the raw feed message (instance)
    /// </summary>
    /// <param name="message">The message to be serialized</param>
    /// <returns>Returns XML message body</returns>
    public static string BuildMessageBody(FeedMessage message)
    {
        if (message is alive aliveMsg)
        {
            return MsgSerializer.SerializeToXml(aliveMsg);
        }

        if (message is odds_change oddsChange)
        {
            return MsgSerializer.SerializeToXml(oddsChange);
        }

        if (message is bet_stop betStop)
        {
            return MsgSerializer.SerializeToXml(betStop);
        }

        if (message is fixture_change fixtureChange)
        {
            return MsgSerializer.SerializeToXml(fixtureChange);
        }

        if (message is snapshot_complete snapshotComplete)
        {
            return MsgSerializer.SerializeToXml(snapshotComplete);
        }

        if (message is bet_settlement betSettlement)
        {
            return MsgSerializer.SerializeToXml(betSettlement);
        }

        return MsgSerializer.SerializeToXml(message);
    }

    /// <summary>
    /// Builds the routing key from the message type
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="sportId">Define sport id for routing key (default 1)</param>
    /// <returns>Returns the appropriate routing key</returns>
    public static string BuildRoutingKey(FeedMessage message, int sportId = 1)
    {
        if (message.GetType() == typeof(alive))
        {
            return "-.-.-.alive.-.-.-.-";
        }

        if (message.GetType() == typeof(odds_change))
        {
            var oddsChange = (odds_change)message;
            var urn = Urn.Parse(oddsChange.event_id);
            return $"hi.{BuildSessionPartOfRoutingKey(oddsChange.product)}.odds_change.{sportId}.{urn.Prefix}:{urn.Type}.{urn.Id}.-";
        }

        if (message.GetType() == typeof(bet_stop))
        {
            var betStop = (bet_stop)message;
            var urn = Urn.Parse(betStop.event_id);
            return $"hi.{BuildSessionPartOfRoutingKey(betStop.product)}.bet_stop.{sportId}.{urn.Prefix}:{urn.Type}.{urn.Id}.-";
        }

        if (message.GetType() == typeof(fixture_change))
        {
            var fixtureChange = (fixture_change)message;
            var urn = Urn.Parse(fixtureChange.event_id);
            return $"hi.pre.live.fixture_change.{sportId}.{urn.Prefix}:{urn.Type}.{urn.Id}.-";
        }

        if (message.GetType() == typeof(snapshot_complete))
        {
            return "-.-.-.snapshot_complete.-.-.-.-";
        }

        if (message.GetType() == typeof(bet_settlement))
        {
            var betSettlement = (bet_settlement)message;
            var urn = Urn.Parse(betSettlement.event_id);
            return $"lo.{BuildSessionPartOfRoutingKey(betSettlement.product)}.bet_settlement.{sportId}.{urn.Prefix}:{urn.Type}.{urn.Id}.-";
        }

        return string.Empty;
    }

    private static string BuildSessionPartOfRoutingKey(int producerId)
    {
        var allProducers = ProducersEndpoint.BuildAll();
        var producer = allProducers.producer.FirstOrDefault(f => f.id == producerId);
        if (producer == null)
        {
            return "missing.missing";
        }

        if (producer.scope.Contains("live", StringComparison.InvariantCultureIgnoreCase))
        {
            return "-.live";
        }

        if (producer.scope.Contains("prematch", StringComparison.InvariantCultureIgnoreCase))
        {
            return "pre.-";
        }

        if (producer.scope.Contains("virtual", StringComparison.InvariantCultureIgnoreCase))
        {
            return "virt.-";
        }

        return "na.na";
    }
}
