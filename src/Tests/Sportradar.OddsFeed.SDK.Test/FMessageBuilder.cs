using System;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Test
{
    /// <summary>
    /// Class for building test feed messages
    /// </summary>
    public class FMessageBuilder
    {
        private readonly int _producerId;

        public FMessageBuilder(int producerId)
        {
            _producerId = producerId;
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
                eventId = new Random().Next();
            }
            return new odds_change
                   {
                       event_id = eventId == null ? "sr:match:1000" : $"sr:match:{eventId}",
                       product = product ?? _producerId,
                       timestamp = SdkInfo.ToEpochTime(timestamp ?? TimeProviderAccessor.Current.Now),
                       request_id = requestId ?? 0
                   };
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
                eventId = new Random().Next();
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
            return new alive
                   {
                       product = product ?? _producerId,
                       subscribed = subscribed ? 1 : 0,
                       timestamp = SdkInfo.ToEpochTime(timestamp ?? TimeProviderAccessor.Current.Now)
                   };
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
            return new snapshot_complete
                   {
                       product = product ?? _producerId,
                       timestamp = SdkInfo.ToEpochTime(timestamp ?? TimeProviderAccessor.Current.Now),
                       request_id = requestId
                   };
        }
    }
}
