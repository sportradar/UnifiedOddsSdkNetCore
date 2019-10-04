/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    public class FeedMessageBuilder
    {
        private readonly IProducer _producer;

        public FeedMessageBuilder(IProducer producer)
        {
            _producer = producer;
        }

        public odds_change BuildOddsChange(long? requestId = null, DateTime? timestamp = null)
        {
            return new odds_change
            {
                event_id = "sr:match:1000",
                product = _producer.Id,
                request_id = requestId ?? 0,
                timestamp = SdkInfo.ToEpochTime(timestamp ?? TimeProviderAccessor.Current.Now),
            };
        }

        public bet_stop BuildBetStop(long? requestId = null, DateTime? timestamp = null)
        {
            return new bet_stop
            {
                event_id = "sr:match:1000",
                product = _producer.Id,
                request_id = requestId ?? 0,
                timestamp = SdkInfo.ToEpochTime(timestamp ?? TimeProviderAccessor.Current.Now),
            };
        }

        public alive BuildAlive(int? product = null, DateTime? timestamp = null, bool subscribed = true)
        {
            return new alive
            {
                subscribed = subscribed ? 1 : 0,
                timestamp = SdkInfo.ToEpochTime(timestamp ?? TimeProviderAccessor.Current.Now),
                product = product ?? _producer.Id
            };
        }

        public snapshot_complete BuildSnapshotComplete(long requestId, DateTime? timestamp = null)
        {
            return new snapshot_complete
            {
                product = _producer.Id,
                timestamp = SdkInfo.ToEpochTime(timestamp ?? TimeProviderAccessor.Current.Now),
                request_id = requestId
            };
        }
    }
}
