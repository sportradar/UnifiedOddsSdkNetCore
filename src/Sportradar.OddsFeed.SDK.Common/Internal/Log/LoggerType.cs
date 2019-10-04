/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Common.Internal.Log
{
    /// <summary>
    /// Enumeration of all possible types of logger used in SDK. Each can have unique settings in log4net.config file.
    /// </summary>
    /// <remarks>Default is basic execution log</remarks>
    public enum LoggerType
    {
        /// <summary>
        /// The execution log
        /// </summary>
        Execution = 0,
        /// <summary>
        /// Log for the feed traffic
        /// </summary>
        FeedTraffic = 1,
        /// <summary>
        /// Log for the rest traffic
        /// </summary>
        RestTraffic = 2,
        /// <summary>
        /// The client iteration log
        /// </summary>
        ClientInteraction = 3,
        /// <summary>
        /// Log for the cache
        /// </summary>
        Cache = 4,
        /// <summary>
        /// Log for the SDK statistics
        /// </summary>
        Stats = 5
    }
}