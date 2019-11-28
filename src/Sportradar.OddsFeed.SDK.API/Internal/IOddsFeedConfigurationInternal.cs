/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes representing internal odds feed configuration / settings
    /// </summary>
    public interface IOddsFeedConfigurationInternal : IOddsFeedConfiguration
    {
        /// <summary>
        /// Gets a value indication whether statistics collection is enabled
        /// </summary>
        bool StatisticsEnabled { get; }

        /// <summary>
        /// Gets the timeout for automatically collecting statistics (in seconds)
        /// </summary>
        int StatisticsTimeout { get; }

        /// <summary>
        /// Gets the limit of records for automatically writing statistics
        /// </summary>
        int StatisticsRecordLimit { get; }

        /// <summary>
        /// Gets the URL of the feed's xReplay Server REST interface
        /// </summary>
        string ReplayApiHost { get; }

        /// <summary>
        /// Gets a <see cref="string"/> representation of Replay API base url
        /// </summary>
        string ReplayApiBaseUrl { get; }

        /// <summary>
        /// Gets the <see cref="string"/> representation of Sports API URI
        /// </summary>
        string ApiBaseUri { get; }

        /// <summary>
        /// Loads the current config object with data retrieved from the Sports API
        /// </summary>
        void Load();

        /// <summary>
        /// Indicates that the SDK will be used to connect to Replay Server
        /// </summary>
        void EnableReplayServer();

        /// <summary>
        /// Gets the bookmaker details
        /// </summary>
        /// <value>The bookmaker details</value>
        IBookmakerDetails BookmakerDetails { get; }
    }
}