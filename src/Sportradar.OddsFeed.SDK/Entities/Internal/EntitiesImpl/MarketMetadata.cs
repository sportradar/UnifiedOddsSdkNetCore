/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Implementation of <see cref="IMarketMetadata"/>
    /// </summary>
    /// <seealso cref="IMarketMetadata" />
    internal class MarketMetadata : IMarketMetadata
    {
        /// <summary>
        /// Gets a timestamp in UTC when to betstop the associated market. Typically used for outrights and typically is the start-time of the event the market refers to
        /// </summary>
        /// <value>The next betstop.</value>
        public long? NextBetstop { get; }

        /// <summary>
        /// Gets the start time of the event (as epoch timestamp)
        /// </summary>
        /// <value>The start time</value>
        public long? StartTime { get; }

        /// <summary>
        /// Gets the end time of the event (as epoch timestamp)
        /// </summary>
        /// <value>The end time</value>
        public long? EndTime { get; }

        /// <summary>
        /// Gets date/time when to betstop the associated market. Typically used for outrights and typically is the start-time of the event the market refers to
        /// </summary>
        /// <value>The next betstop</value>
        public DateTime? NextBetstopDate { get; }

        /// <summary>
        /// Gets the start time of the event
        /// </summary>
        /// <value>The start time</value>
        public DateTime? StartTimeDate { get; }

        /// <summary>
        /// Gets the end time of the event
        /// </summary>
        /// <value>The end time</value>
        public DateTime? EndTimeDate { get; }

        /// <summary>
        /// Gets the Italian AAMS id for this outright
        /// </summary>
        /// <value>The Italian AAMS id for this outright</value>
        public long? AamsId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketMetadata"/> class
        /// </summary>
        /// <param name="marketMetadata">The market meta data</param>
        public MarketMetadata(marketMetadata marketMetadata)
        {
            if (marketMetadata != null)
            {
                if (marketMetadata.next_betstopSpecified)
                {
                    NextBetstop = marketMetadata.next_betstop;
                    NextBetstopDate = SdkInfo.TryFromEpochTime(marketMetadata.next_betstop);
                }
                if (marketMetadata.start_timeSpecified)
                {
                    StartTime = marketMetadata.start_time;
                    StartTimeDate = SdkInfo.TryFromEpochTime(marketMetadata.start_time);
                }
                if (marketMetadata.end_timeSpecified)
                {
                    EndTime = marketMetadata.end_time;
                    EndTimeDate = SdkInfo.TryFromEpochTime(marketMetadata.end_time);
                }
                if (marketMetadata.aams_idSpecified)
                {
                    AamsId = marketMetadata.aams_id;
                }
            }
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"MarketMetaData=[NextBetstop={NextBetstopDate}, StartTime={StartTimeDate}, EndTime={EndTimeDate}, AamsId={AamsId}]";
        }
    }
}
